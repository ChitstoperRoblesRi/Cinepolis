using System.Data.Common;
using Cinepolis.Application;
using Cinepolis.Domain;
using Dapper;

namespace Cinepolis.Infrastructure;

/// <summary>Ejecuta las operaciones de venta sobre la conexión y transacción de la unidad de trabajo.</summary>
internal sealed class TransaccionOrden(DbConnection db, DbTransaction tx, DulceriaRepository dulceria,
    CancellationToken token) : ITransaccionOrden
{
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

    private CommandDefinition Cmd(string sql, object args) => new(sql, args, tx, cancellationToken: token);

    /// <inheritdoc/>
    public async Task<DisponibilidadCompra> TomarReservasAsync(Checkout compra, string conexion, int? socioId)
    {
        var ids = compra.Asientos.Select(b => b.AsientoId).ToArray();
        var args = new { compra.FuncionId, ids, Tipos = compra.Asientos.Select(b => b.TipoBoletoId).ToArray(),
            Titular = "sala:" + conexion, socioId, Activo = "activo", Convertido = "convertido", Reservado = "reservado", Boleto = "boleto" };
        var bloqueados = (await db.QueryAsync<int>(Cmd(Bloquear, args))).AsList();
        var convertidos = (await db.QueryAsync<int>(Cmd(Convertir, args))).AsList();
        var tarifas = (await db.QueryAsync<Tarifa>(Cmd(Tarifas, args))).AsList();
        return new(bloqueados, convertidos, tarifas);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SeleccionCatalogo>> CatalogoAsync(Checkout compra)
    {
        var productos = new List<SeleccionCatalogo>();
        if (compra.Productos.Length > 0)
        {
            var complejoId = await db.QuerySingleAsync<int>(Cmd(
                "SELECT s.complejo_id FROM public.funcion f JOIN public.sala s USING (sala_id) WHERE f.funcion_id = @FuncionId", new { compra.FuncionId }));
            foreach (var grupo in compra.Productos.GroupBy(p => p.CatalogoId))
            {
                var catalogo = (await dulceria.LeerProductos(db, complejoId,
                    grupo.Select(p => p.ProductoId).Distinct().ToArray(), tx, token, grupo.Key)).ToDictionary(p => p.ProductoId);
                foreach (var linea in grupo)
                {
                    if (!catalogo.TryGetValue(linea.ProductoId, out var p)) throw new ConflictoCompra("Producto no disponible en este complejo.");
                    productos.Add(new(linea, p));
                }
            }
        }

        return productos;
    }

    /// <inheritdoc/>
    public async Task<SaldoClub?> ClubAsync(int? socioId)
    {
        SaldoClub? club = null;
        if (socioId.HasValue)
        {
            club = await db.QuerySingleOrDefaultAsync<SaldoClub>(Cmd("""
                SELECT c.numero_tarjeta, c.puntos_acumulados AS saldo, n.porcentaje_acumulacion_puntos AS porcentaje,
                    c.puntos_vencen_fecha >= CURRENT_DATE AS vigente
                FROM public.cuenta_club c JOIN public.nivel_club n USING (nivel_id)
                WHERE c.socio_id = @SocioId FOR UPDATE OF c
                """, new { socioId })) ?? throw new ConflictoCompra("La cuenta Club ya no existe.");
        }
        return club;
    }

    /// <inheritdoc/>
    public async Task<Ticket> GuardarAsync(OrdenCotizada orden, PagoCompra pago, int? socioId, decimal? puntos, string movimiento)
    {
        var (cargo, _, total, folio, boletos, productos) = orden;
        var ids = boletos.Select(b => b.AsientoId).ToArray();
        int ventaId = await db.QuerySingleAsync<int>(Cmd("""
            INSERT INTO public.venta (importe_total, cargo_servicio, forma_pago, tipo_venta, socio_id, nombre_comprador, correo_comprador)
            VALUES (@Total, @Cargo, @FormaPago, @Canal, @SocioId, @Nombre, @Correo) RETURNING venta_id
            """, new { total, cargo, pago.FormaPago, pago.Nombre, pago.Correo, Canal = "Online", socioId }));
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
        if (puntos.HasValue)
        {
            await db.ExecuteAsync(Cmd("""
                INSERT INTO public.detalle_puntos (venta_id, socio_id, cantidad_puntos, tipo_movimiento)
                VALUES (@VentaId, @SocioId, @Puntos, @Movimiento)
                """, new { ventaId, socioId, Puntos = puntos,
                    Movimiento = movimiento }));
        }
        await db.ExecuteAsync(Cmd("""
            INSERT INTO public.folio (codigo, tipo_folio, tipo_uso, venta_id, socio_id)
            VALUES (@Folio, @Tipo, @Uso, @VentaId, @SocioId)
            """, new { folio, Tipo = "Cinepass", Uso = "boletos", ventaId, socioId }));
        var ticket = await OrdenRepository.Consultar(db, tx, folio, socioId, token) ?? throw new InvalidOperationException("Falta el ticket de la venta.");
        return ticket;
    }

}
