using Cinepolis.Application;
using Cinepolis.Domain;
using Dapper;
using System.Data.Common;

namespace Cinepolis.Infrastructure;

/// <summary>Accede al estado de las butacas de una función.</summary>
public sealed class AsientoRepository(IDbConnectionFactory factory) : IFuncionAsientosRepository, IReservaAsientosRepository, IReservasVencidasRepository
{
    private const string PrefijoReserva = "sala:";
    private const int MinutosReserva = 7;

    /// <summary>Recupera la geometría, disponibilidad y tarifas de la sala.</summary>
    public async Task<MapaSala?> LeerPorFuncionAsync(int funcionId, CancellationToken token)
    {
        const string sql = """
            SELECT s.sala_id, s.complejo_id, s.nombre_sala, s.tipo_sala, s.capacidad_asientos
            FROM public.funcion f JOIN public.sala s ON s.sala_id = f.sala_id
            WHERE f.funcion_id = @FuncionId;

            SELECT a.asiento_id, a.fila, a.columna, a.asiento_api_id AS numero,
                a.tipo_asiento, a.estado, a.area_category_code,
                (a.tipo_asiento = @SillaDeRuedas OR
                    (f.sala_id = @SalaAcaya4 AND a.fila = @FilaAccesible
                     AND a.asiento_api_id IS NOT NULL AND a.asiento_api_id = ANY(@NumerosAccesibles))) AS es_accesible
            FROM public.asiento a JOIN public.funcion f USING (funcion_id)
            WHERE a.funcion_id = @FuncionId ORDER BY a.fila, a.columna DESC;

            SELECT t.tipo_boleto_id, t.nombre, area.area_category_code, coalesce(t.precio_fijo, p.precio) AS precio, p.cargo_servicio_online AS cargo
            FROM public.funcion f JOIN public.sala s USING (sala_id)
            CROSS JOIN (SELECT DISTINCT area_category_code FROM public.asiento WHERE funcion_id = @FuncionId) area
            CROSS JOIN public.tipo_boleto t
            JOIN LATERAL (SELECT b.precio, b.cargo_servicio_online FROM public.precio_boleto b
                WHERE b.complejo_id = s.complejo_id AND b.formato_id = f.formato_id AND b.tipo_sala = s.tipo_sala
                  AND b.tipo_boleto_id = t.tipo_boleto_id AND (b.area_category_code = area.area_category_code OR b.area_category_code IS NULL)
                ORDER BY b.area_category_code NULLS LAST LIMIT 1) p ON TRUE
            WHERE f.funcion_id = @FuncionId AND f.activa AND f.fecha_funcion + f.hora_inicio > LOCALTIMESTAMP
              AND NOT t.es_promocional
            ORDER BY t.nombre, p.precio
            """;
        await using var db = await factory.OpenAsync(token);
        // La plantilla de Acaya aún clasifica A5 y A6 como tradicionales.
        var parametros = new { funcionId, SillaDeRuedas = AsientoTipoAsiento.SillaDeRuedas.TextoSql(),
            SalaAcaya4 = 39, FilaAccesible = "A", NumerosAccesibles = new[] { "5", "6" } };
        using var datos = await db.QueryMultipleAsync(new CommandDefinition(sql, parametros, cancellationToken: token));
        var sala = await datos.ReadSingleOrDefaultAsync<SalaLectura>();
        var asientos = (await datos.ReadAsync<AsientoLectura>()).AsList();
        var tarifas = (await datos.ReadAsync<TarifaFuncion>()).AsList();
        return sala is null ? null : new(funcionId, sala, asientos, tarifas);
    }

    private static Task<int?> TomarAsientoAsync(DbConnection db, DbTransaction tx, int funcionId, int asientoId, CancellationToken token)
    {
        const string sql = """
            SELECT asiento_id FROM public.asiento
            WHERE funcion_id = @FuncionId AND asiento_id = @AsientoId FOR UPDATE
            """;
        return db.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(sql, new { funcionId, asientoId }, tx, cancellationToken: token));
    }

    // Los llamadores ya tienen los cerrojos de los asientos. Una sentencia nueva bajo READ COMMITTED
    // ve el carrito confirmado por el titular anterior, incluso si hubo que esperar su cerrojo.
    private static async Task<IReadOnlyList<CambioEstadoAsiento>> CerrarReservasAsync(
        DbConnection db, DbTransaction tx, int[] ids, string? titular, CancellationToken token)
    {
        const string sql = """
            WITH cerrados AS (
                UPDATE public.carrito c SET estado = @Abandonado
                FROM public.detalle_carrito d JOIN public.asiento a ON a.asiento_id = d.asiento_id
                WHERE c.carrito_id = d.carrito_id AND a.asiento_id = ANY(@Ids)
                  AND a.estado = @Reservado AND c.estado = @Activo AND d.tipo_item = @Boleto
                  AND starts_with(c.uuid_sesion_web, @Prefijo)
                  AND ((CAST(@Titular AS text) IS NULL AND c.fecha_expiracion <= LOCALTIMESTAMP)
                       OR c.uuid_sesion_web = @Titular)
                RETURNING d.asiento_id
            )
            UPDATE public.asiento a SET estado = @Disponible
            FROM cerrados WHERE a.asiento_id = cerrados.asiento_id AND a.estado = @Reservado
            RETURNING a.funcion_id, a.asiento_id, a.estado
            """;
        var parametros = new { ids, titular, Prefijo = PrefijoReserva, Abandonado = "abandonado", Activo = "activo",
            Boleto = "boleto", Reservado = AsientoEstado.Reservado.TextoSql(), Disponible = AsientoEstado.Disponible.TextoSql() };
        return (await db.QueryAsync<CambioEstadoAsiento>(new CommandDefinition(sql, parametros, tx, cancellationToken: token))).AsList();
    }

    /// <summary>Reserva una butaca disponible para la conexión solicitante.</summary>
    public async Task<bool> SeleccionarAsync(int funcionId, int asientoId, string conexion, CancellationToken token)
    {
        const string tomar = """
            UPDATE public.asiento SET estado = @Reservado
            WHERE asiento_id = @AsientoId AND funcion_id = @FuncionId AND estado = @Disponible
            """;
        const string crear = """
            WITH nuevo AS (
                INSERT INTO public.carrito (uuid_sesion_web, fecha_expiracion, estado)
                VALUES (@Titular, LOCALTIMESTAMP + make_interval(mins => @MinutosReserva), @Activo) RETURNING carrito_id
            )
            INSERT INTO public.detalle_carrito (carrito_id, tipo_item, cantidad, precio_unitario, asiento_id)
            SELECT carrito_id, @Boleto, @Cantidad, @PrecioProvisional, @AsientoId FROM nuevo
            """;
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        if (await TomarAsientoAsync(db, tx, funcionId, asientoId, token) is null) return false;
        await CerrarReservasAsync(db, tx, [asientoId], null, token);
        var parametros = new { funcionId, asientoId, Reservado = AsientoEstado.Reservado.TextoSql(),
            Disponible = AsientoEstado.Disponible.TextoSql(), Titular = PrefijoReserva + conexion, MinutosReserva,
            Activo = "activo", Boleto = "boleto", Cantidad = 1, PrecioProvisional = 0m };
        if (await db.ExecuteAsync(new CommandDefinition(tomar, parametros, tx, cancellationToken: token)) != 1) return false;
        await db.ExecuteAsync(new CommandDefinition(crear, parametros, tx, cancellationToken: token));
        await tx.CommitAsync(token);
        return true;
    }

    /// <summary>Libera una butaca reservada por la conexión solicitante.</summary>
    public async Task<bool> LiberarAsync(int funcionId, int asientoId, string conexion, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        if (await TomarAsientoAsync(db, tx, funcionId, asientoId, token) is null) return false;
        var cambios = await CerrarReservasAsync(db, tx, [asientoId], PrefijoReserva + conexion, token);
        await tx.CommitAsync(token);
        return cambios.Count != 0;
    }

    /// <summary>Libera las reservas caducadas y devuelve los cambios confirmados.</summary>
    public async Task<IReadOnlyList<CambioEstadoAsiento>> LiberarVencidosAsync(CancellationToken token)
    {
        const string sql = """
            SELECT a.asiento_id
            FROM public.carrito c
            JOIN public.detalle_carrito d ON d.carrito_id = c.carrito_id
            JOIN public.asiento a ON a.asiento_id = d.asiento_id
            WHERE c.estado = @Activo AND c.fecha_expiracion <= LOCALTIMESTAMP
              AND starts_with(c.uuid_sesion_web, @Prefijo) AND d.tipo_item = @Boleto AND a.estado = @Reservado
            ORDER BY a.asiento_id LIMIT @Lote FOR UPDATE OF a SKIP LOCKED
            """;
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        var parametros = new { Activo = "activo", Prefijo = PrefijoReserva, Boleto = "boleto",
            Reservado = AsientoEstado.Reservado.TextoSql(), Lote = 256 };
        var ids = (await db.QueryAsync<int>(new CommandDefinition(sql, parametros, tx, cancellationToken: token))).ToArray();
        if (ids.Length == 0) return [];
        var cambios = await CerrarReservasAsync(db, tx, ids, null, token);
        await tx.CommitAsync(token);
        return cambios;
    }
}
