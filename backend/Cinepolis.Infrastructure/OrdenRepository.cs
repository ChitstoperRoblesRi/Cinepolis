using Cinepolis.Domain;
using Cinepolis.Application;
using Dapper;
using System.Data.Common;

namespace Cinepolis.Infrastructure;

/// <summary>Persiste compras confirmadas y recupera sus comprobantes.</summary>
public sealed class OrdenRepository(IDbConnectionFactory factory) : IOrdenRepository
{
    /// <summary>Lista los folios de las compras del titular.</summary>
    public async Task<IEnumerable<string>> FoliosAsync(int socioId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        return await db.QueryAsync<string>(new CommandDefinition("""
            SELECT f.codigo FROM public.folio f JOIN public.venta v USING (venta_id)
            WHERE v.socio_id = @SocioId AND starts_with(f.codigo, @Prefijo) ORDER BY v.fecha_venta DESC
            """, new { socioId, FolioCompra.Prefijo }, cancellationToken: token));
    }

    /// <summary>Recupera el ticket si el titular puede consultarlo.</summary>
    public async Task<Ticket?> LeerAsync(string folio, int? socioId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        return await Consultar(db, null, folio, socioId, token);
    }

    internal static async Task<Ticket?> Consultar(DbConnection db, DbTransaction? tx, string folio, int? socioId, CancellationToken token)
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
