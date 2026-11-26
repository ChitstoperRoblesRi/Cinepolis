using System.Security.Cryptography;
using Cinepolis.Application;
using Dapper;
using Npgsql;

namespace Cinepolis.Infrastructure;

public sealed class OrdenRepository(NpgsqlConnectionFactory factory) : IOrdenRepository
{
    private sealed record Tarifa(int AsientoId, int TipoBoletoId, decimal Precio, decimal Cargo);
    private sealed record Articulo(int ProductoId, int Cantidad, decimal Precio, int[] Opciones);
    private sealed record Club(string NumeroTarjeta, int Saldo, decimal Porcentaje, bool Vigente);
    private const string Bloquear = """
        SELECT asiento_id FROM public.asiento WHERE funcion_id = @FuncionId AND asiento_id = ANY(@Ids)
        ORDER BY asiento_id FOR UPDATE
        """;
    private const string Convertir = """
        UPDATE public.carrito c SET estado = @Convertido, socio_id = @SocioId
        FROM public.detalle_carrito d JOIN public.asiento a ON a.asiento_id = d.asiento_id
        WHERE d.carrito_id = c.carrito_id AND d.tipo_item = @Boleto AND a.asiento_id = ANY(@Ids)
          AND a.estado = @Reservado AND c.estado = @Activo AND c.uuid_sesion_web = @Titular
          AND c.fecha_expiracion > clock_timestamp()::timestamp
        RETURNING a.asiento_id
        """;
    private const string Tarifas = """
        SELECT a.asiento_id, t.tipo_boleto_id, coalesce(t.precio_fijo, p.precio) AS precio,
            p.cargo_servicio_online AS cargo
        FROM unnest(@Ids, @Tipos) AS i(asiento_id, tipo_boleto_id)
        JOIN public.asiento a USING (asiento_id) JOIN public.funcion f USING (funcion_id)
        JOIN public.sala s USING (sala_id) JOIN public.tipo_boleto t ON t.tipo_boleto_id = i.tipo_boleto_id
        JOIN LATERAL (SELECT b.precio, b.cargo_servicio_online FROM public.precio_boleto b
            WHERE b.complejo_id = s.complejo_id AND b.formato_id = f.formato_id AND b.tipo_sala = s.tipo_sala
              AND b.tipo_boleto_id = t.tipo_boleto_id AND (b.area_category_code = a.area_category_code OR b.area_category_code IS NULL)
            ORDER BY b.area_category_code NULLS LAST LIMIT 1) p ON TRUE
        WHERE f.activa AND f.fecha_funcion + f.hora_inicio > LOCALTIMESTAMP AND NOT t.es_promocional
        """;
    private static Articulo Cotizar(ProductoCompra linea, ProductoDto p)
    {
        var ids = linea.Opciones ?? [];
        var opciones = p.Reglas.SelectMany(r => r.Opciones).Where(o => ids.Contains(o.OpcionId)).ToArray();
        if (opciones.Length != ids.Length || p.Reglas.Any(r =>
        {
            int n = r.Opciones.Count(o => ids.Contains(o.OpcionId));
            var maximo = r.Max;
            if (r.BloquePalomitas.HasValue && r.TipoModificador == "flavor")
            {
                var paraLlevar = p.Reglas.Where(t => t.BloquePalomitas == r.BloquePalomitas && t.TipoModificador == "size")
                    .SelectMany(t => t.Opciones).Any(o => ids.Contains(o.OpcionId) &&
                        o.Nombre.Trim().Equals("Para llevar", StringComparison.OrdinalIgnoreCase));
                maximo = Math.Min(r.Max, paraLlevar ? 4 : 2);
            }
            return n < r.Min || n > maximo;
        })) throw new ConflictoCompra("Revisa las opciones de " + p.NombreProducto + ".");
        var variante = opciones.SingleOrDefault(o => o.PrecioVariante.HasValue);
        var precio = variante?.PrecioVariante ?? p.Precio!.Value;
        foreach (var regla in p.Reglas)
        {
            var extras = regla.Opciones.Where(o => ids.Contains(o.OpcionId) && !o.PrecioVariante.HasValue)
                .Select(o => o.Extra);
            precio += regla.BloquePalomitas.HasValue && regla.TipoModificador == "flavor"
                ? extras.DefaultIfEmpty().Max() : extras.Sum();
        }
        if (precio is < 0m or > 999999.99m) throw new ConflictoCompra("Importe de artículo fuera de rango.");
        return new(p.ProductoId, linea.Cantidad, precio, ids);
    }

    public async Task<Ticket> ComprarAsync(Checkout compra, string conexion, int? socioId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        await using NpgsqlTransaction tx = await db.BeginTransactionAsync(token);
        CommandDefinition Cmd(string sql, object args) => new(sql, args, tx, cancellationToken: token);
        var ids = compra.Asientos.Select(b => b.AsientoId).ToArray();
        var args = new { compra.FuncionId, ids, Tipos = compra.Asientos.Select(b => b.TipoBoletoId).ToArray(),
            Titular = "sala:" + conexion, socioId, Activo = "activo", Convertido = "convertido", Reservado = "reservado", Boleto = "boleto" };
        var bloqueados = (await db.QueryAsync<int>(Cmd(Bloquear, args))).AsList();
        var convertidos = (await db.QueryAsync<int>(Cmd(Convertir, args))).AsList();
        if (bloqueados.Count != ids.Length || convertidos.Count != ids.Length || convertidos.Distinct().Count() != ids.Length)
            throw new ConflictoCompra("Uno o más asientos carecen de una reserva vigente de esta sesión.");

        var boletos = (await db.QueryAsync<Tarifa>(Cmd(Tarifas, args))).AsList();
        if (boletos.Count != ids.Length) throw new ConflictoCompra("Función o tarifa de boleto no disponible.");
        var productos = new List<Articulo>();
        if (compra.Productos.Length > 0)
        {
            var complejoId = await db.QuerySingleAsync<int>(Cmd(
                "SELECT s.complejo_id FROM public.funcion f JOIN public.sala s USING (sala_id) WHERE f.funcion_id = @FuncionId", args));
            foreach (var grupo in compra.Productos.GroupBy(p => p.CatalogoId))
            {
                var catalogo = (await DulceriaRepository.LeerProductos(db, complejoId,
                    grupo.Select(p => p.ProductoId).Distinct().ToArray(), tx, token, grupo.Key)).ToDictionary(p => p.ProductoId);
                foreach (var linea in grupo)
                {
                    if (!catalogo.TryGetValue(linea.ProductoId, out var p)) throw new ConflictoCompra("Producto no disponible en este complejo.");
                    productos.Add(Cotizar(linea, p));
                }
            }
        }

        decimal cargo = boletos.Sum(b => b.Cargo);
        decimal subtotal = boletos.Sum(b => b.Precio) + productos.Sum(p => p.Cantidad * p.Precio);
        decimal total = subtotal + cargo;
        if (total > 99999999.99m || cargo > 999999.99m) throw new ConflictoCompra("El importe supera el máximo permitido.");
        if (compra.Pago.TotalEsperado is decimal esperado && esperado != total)
            throw new ConflictoCompra("El precio cambió. Revisa las tarifas y la dulcería antes de confirmar.");
        bool canje = compra.Pago.FormaPago == "Puntos";
        Club? club = null;
        if (socioId.HasValue)
        {
            club = await db.QuerySingleOrDefaultAsync<Club>(Cmd("""
                SELECT c.numero_tarjeta, c.puntos_acumulados AS saldo, n.porcentaje_acumulacion_puntos AS porcentaje,
                    c.puntos_vencen_fecha >= CURRENT_DATE AS vigente
                FROM public.cuenta_club c JOIN public.nivel_club n USING (nivel_id)
                WHERE c.socio_id = @SocioId FOR UPDATE OF c
                """, new { socioId })) ?? throw new ConflictoCompra("La cuenta Club ya no existe.");
        }
        if (compra.Pago.TarjetaClub is string tarjeta && tarjeta != club?.NumeroTarjeta)
            throw new ConflictoCompra("Inicia sesión con la cuenta titular de la tarjeta Club.");
        if (canje && (club is null || !club.Vigente || club.Saldo < total))
            throw new ConflictoCompra("El saldo vigente de Club no cubre el total de la compra.");

        int ventaId = await db.QuerySingleAsync<int>(Cmd("""
            INSERT INTO public.venta (importe_total, cargo_servicio, forma_pago, tipo_venta, socio_id, nombre_comprador, correo_comprador)
            VALUES (@Total, @Cargo, @FormaPago, @Canal, @SocioId, @Nombre, @Correo) RETURNING venta_id
            """, new { total, cargo, compra.Pago.FormaPago, compra.Pago.Nombre, compra.Pago.Correo, Canal = "Online", socioId }));
        await db.ExecuteAsync(Cmd("""
            INSERT INTO public.detalle_venta_boleto (venta_id, asiento_id, tipo_boleto_id, precio_unitario)
            SELECT @VentaId, i.* FROM unnest(@Ids, @Tipos, @Precios) i
            """, new { ventaId, Ids = boletos.Select(b => b.AsientoId).ToArray(),
                Tipos = boletos.Select(b => b.TipoBoletoId).ToArray(), Precios = boletos.Select(b => b.Precio).ToArray() }));
        foreach (var p in productos)
        {
            var detalleId = await db.QuerySingleAsync<int>(Cmd("""
                INSERT INTO public.detalle_venta_producto (venta_id, producto_id, cantidad, precio_unitario)
                VALUES (@VentaId, @ProductoId, @Cantidad, @Precio) RETURNING detalle_producto_id
                """, new { ventaId, p.ProductoId, p.Cantidad, p.Precio }));
            if (p.Opciones.Length > 0)
                await db.ExecuteAsync(Cmd("""
                    INSERT INTO public.personalizacion_venta (detalle_venta_producto_id, opcion_id, cantidad)
                    SELECT @DetalleId, opcion, @Cantidad FROM unnest(@Opciones) AS opcion
                    """, new { detalleId, p.Opciones, p.Cantidad }));
        }
        await db.ExecuteAsync(Cmd("UPDATE public.asiento SET estado = @Vendido WHERE asiento_id = ANY(@Ids)",
            new { ids, Vendido = "vendido" }));
        if (club is not null)
        {
            await db.ExecuteAsync(Cmd("""
                INSERT INTO public.detalle_puntos (venta_id, socio_id, cantidad_puntos, tipo_movimiento)
                VALUES (@VentaId, @SocioId, @Puntos, @Movimiento)
                """, new { ventaId, socioId, Puntos = canje ? total : decimal.Round(subtotal * club.Porcentaje / 100, 2, MidpointRounding.AwayFromZero),
                    Movimiento = canje ? "canje" : "acumulacion" }));
        }
        string folio = "WEB-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
        await db.ExecuteAsync(Cmd("""
            INSERT INTO public.folio (codigo, tipo_folio, tipo_uso, venta_id, socio_id)
            VALUES (@Folio, @Tipo, @Uso, @VentaId, @SocioId)
            """, new { folio, Tipo = "Cinepass", Uso = "boletos", ventaId, socioId }));
        var ticket = await Consultar(db, tx, folio, socioId, token) ?? throw new InvalidOperationException("Falta el ticket de la venta.");
        await tx.CommitAsync(token);
        return ticket;
    }

    public async Task<IEnumerable<string>> FoliosAsync(int socioId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        return await db.QueryAsync<string>(new CommandDefinition("""
            SELECT f.codigo FROM public.folio f JOIN public.venta v USING (venta_id)
            WHERE v.socio_id = @SocioId AND starts_with(f.codigo, 'WEB-') ORDER BY v.fecha_venta DESC
            """, new { socioId }, cancellationToken: token));
    }

    public async Task<Ticket?> LeerAsync(string folio, int? socioId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        return await Consultar(db, null, folio, socioId, token);
    }

    private static async Task<Ticket?> Consultar(NpgsqlConnection db, NpgsqlTransaction? tx, string folio, int? socioId, CancellationToken token)
    {
        const string sql = """
            SELECT f.codigo AS folio, f.estado, v.fecha_venta, v.forma_pago, v.cargo_servicio, v.importe_total, v.venta_id
            FROM public.folio f JOIN public.venta v USING (venta_id)
            WHERE f.codigo = @Folio AND (v.socio_id IS NULL OR v.socio_id = @SocioId)
            """;
        var head = (await db.QueryAsync<CabeceraTicket, int, (CabeceraTicket Orden, int Id)>(
            new CommandDefinition(sql, new { folio, socioId }, tx, cancellationToken: token),
            (h, id) => (h, id), splitOn: "venta_id")).SingleOrDefault();
        if (head.Orden is null) return null;
        const string lineas = """
            SELECT a.funcion_id, a.asiento_id, a.fila, a.columna,
                a.asiento_api_id AS numero,
                t.nombre AS tipo_boleto, d.precio_unitario,
                s.nombre_sala, c.nombre_comercial, p.titulo, to_char(f.fecha_funcion + f.hora_inicio, 'YYYY-MM-DD HH24:MI') AS horario
            FROM public.detalle_venta_boleto d JOIN public.asiento a USING (asiento_id)
            JOIN public.funcion f ON f.funcion_id = a.funcion_id JOIN public.sala s ON s.sala_id = f.sala_id
            JOIN public.complejo c ON c.complejo_id = s.complejo_id JOIN public.pelicula p ON p.pelicula_id = f.pelicula_id
            JOIN public.tipo_boleto t USING (tipo_boleto_id) WHERE d.venta_id = @Id ORDER BY a.fila, a.columna;
            SELECT p.producto_id, p.nombre_producto, d.cantidad, d.precio_unitario,
                ARRAY(SELECT o.nombre_opcion::text FROM public.personalizacion_venta pv JOIN public.modificador_opcion o USING (opcion_id)
                    WHERE pv.detalle_venta_producto_id = d.detalle_producto_id ORDER BY o.opcion_id) AS opciones FROM public.detalle_venta_producto d
            JOIN public.producto_dulceria p USING (producto_id) WHERE d.venta_id = @Id ORDER BY d.detalle_producto_id;
            SELECT coalesce(sum(cantidad_puntos), 0) FROM public.detalle_puntos WHERE venta_id = @Id AND tipo_movimiento = @Movimiento
            """;
        using var rows = await db.QueryMultipleAsync(new CommandDefinition(lineas, new { head.Id, Movimiento = "acumulacion" }, tx, cancellationToken: token));
        var boletos = (await rows.ReadAsync<BoletoTicket>()).AsList();
        var productos = (await rows.ReadAsync<ProductoTicket>()).AsList();
        return new(head.Orden, boletos.Sum(b => b.PrecioUnitario), productos.Sum(p => p.PrecioUnitario * p.Cantidad),
            await rows.ReadSingleAsync<decimal>(), boletos, productos);
    }
}
