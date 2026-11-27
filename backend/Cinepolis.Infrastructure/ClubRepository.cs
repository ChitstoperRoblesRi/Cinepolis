using Npgsql;
using Cinepolis.Domain;
using System.Security.Cryptography;
using Cinepolis.Application;
using Dapper;
using System.Data.Common;

namespace Cinepolis.Infrastructure;

/// <summary>Accede a códigos, cuentas y compras del programa Club.</summary>
public sealed class ClubRepository(IDbConnectionFactory factory) : IAccesoClubRepository, IClubQueryRepository
{
    private static Task SerializarCorreo(DbConnection db, DbTransaction tx, string correo, CancellationToken token) =>
        db.ExecuteAsync(new CommandDefinition("SELECT pg_advisory_xact_lock(hashtextextended(@Correo, 0))",
            new { correo }, tx, cancellationToken: token));

    /// <summary>Registra un código si ha transcurrido el intervalo de reenvío.</summary>
    public async Task<bool> CrearCodigoAsync(string correo, string codigo, CancellationToken token)
    {
        const string reciente = """
            SELECT EXISTS (SELECT FROM public.otp_verificacion WHERE correo = @Correo
                AND expiracion > LOCALTIMESTAMP + make_interval(mins => @Umbral))
            """;
        const string renovar = """
            UPDATE public.otp_verificacion SET usado = TRUE WHERE correo = @Correo AND NOT usado;
            INSERT INTO public.otp_verificacion (correo, codigo, expiracion)
            VALUES (@Correo, @Codigo, LOCALTIMESTAMP + make_interval(mins => @Minutos))
            """;
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        await SerializarCorreo(db, tx, correo, token);
        var args = new { correo, codigo, Minutos = 5, Umbral = 4 };
        if (await db.ExecuteScalarAsync<bool>(new CommandDefinition(reciente, args, tx, cancellationToken: token))) return false;
        await db.ExecuteAsync(new CommandDefinition(renovar, args, tx, cancellationToken: token));
        await tx.CommitAsync(token);
        return true;
    }

    /// <summary>Valida el código y devuelve la identidad del socio.</summary>
    public async Task<int?> AutenticarAsync(Login login, Registro? registro, CancellationToken token)
    {
        const string verificar = """
            UPDATE public.otp_verificacion SET intentos = intentos + 1, usado = codigo = @Codigo
            WHERE otp_id = (SELECT otp_id FROM public.otp_verificacion WHERE correo = @Correo
                ORDER BY otp_id DESC LIMIT 1)
              AND NOT usado AND intentos < @MaxIntentos AND expiracion > clock_timestamp()::timestamp
            RETURNING usado
            """;
        const string buscar = "SELECT socio_id FROM public.cuenta_club WHERE lower(correo) = @Correo";
        const string insertar = """
            INSERT INTO public.cuenta_club (correo, usuario_cc, numero_tarjeta, nombre, apellidos,
                fecha_nacimiento, puntos_vencen_fecha, semestre_actual, nivel_id)
            SELECT @Correo, @UsuarioCc, @Tarjeta, @Nombre, @Apellidos, @FechaNacimiento,
                (CURRENT_DATE + make_interval(years => @Vigencia))::date,
                concat(extract(year from CURRENT_DATE), '-', CASE WHEN extract(month from CURRENT_DATE) <= 6 THEN 1 ELSE 2 END),
                nivel_id FROM public.nivel_club WHERE visitas_min_semestre = 0 ORDER BY nivel_id LIMIT 1
            RETURNING socio_id
            """;
        string correo = login.Correo.Trim().ToLowerInvariant();
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        await SerializarCorreo(db, tx, correo, token);
        var valido = await db.QuerySingleOrDefaultAsync<bool>(new CommandDefinition(verificar,
            new { correo, login.Codigo, MaxIntentos = 3 }, tx, cancellationToken: token));
        int? socio = null;
        if (valido)
        {
            var existentes = (await db.QueryAsync<int>(new CommandDefinition(buscar, new { correo }, tx, cancellationToken: token))).AsList();
            if (registro is null && existentes.Count == 1) socio = existentes[0];
            if (registro is not null)
            {
                if (existentes.Count != 0) throw new ConflictoCompra("El correo ya tiene una cuenta Club.");
                var args = new { correo, registro.UsuarioCc, registro.Nombre, registro.Apellidos, registro.FechaNacimiento,
                    Tarjeta = Convert.ToHexString(RandomNumberGenerator.GetBytes(10)), Vigencia = 1 };
                try
                {
                    socio = await db.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(insertar, args, tx, cancellationToken: token))
                        ?? throw new ConflictoCompra("Falta un nivel Club inicial.");
                }
                catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new ConflictoCompra("El identificador Club ya existe.");
                }
            }
        }
        await tx.CommitAsync(token);
        return socio;
    }

    /// <summary>Recupera el historial de compras del socio autenticado.</summary>
    public async Task<IEnumerable<CompraClub>> ComprasAsync(int socioId, CancellationToken token)
    {
        const string sql = """
            SELECT fo.codigo AS folio, p.titulo, c.nombre_comercial AS complejo,
                f.fecha_funcion + f.hora_inicio AS horario, v.importe_total AS total, fo.estado,
                (fo.estado = @Activo AND f.activa AND LOCALTIMESTAMP < f.fecha_funcion + f.hora_termino
                    + CASE WHEN f.hora_termino < f.hora_inicio THEN interval '1 day' ELSE interval '0' END) AS activa
            FROM public.venta v JOIN public.folio fo USING (venta_id)
            JOIN LATERAL (SELECT DISTINCT a.funcion_id FROM public.detalle_venta_boleto d
                JOIN public.asiento a USING (asiento_id) WHERE d.venta_id = v.venta_id) b ON true
            JOIN public.funcion f USING (funcion_id)
            JOIN public.pelicula p ON p.pelicula_id = f.pelicula_id JOIN public.sala s ON s.sala_id = f.sala_id
            JOIN public.complejo c ON c.complejo_id = s.complejo_id
            WHERE v.socio_id = @SocioId AND starts_with(fo.codigo, @Prefijo)
            ORDER BY v.fecha_venta DESC, fo.codigo
            """;
        await using var db = await factory.OpenAsync(token);
        return await db.QueryAsync<CompraClub>(new CommandDefinition(sql, new { socioId, Activo = "activo", FolioCompra.Prefijo }, cancellationToken: token));
    }

    /// <summary>Recupera los datos y beneficios de la cuenta del socio.</summary>
    public async Task<PerfilClub?> PerfilAsync(int socioId, CancellationToken token)
    {
        const string sql = """
            SELECT c.socio_id, c.usuario_cc, c.numero_tarjeta, c.nombre, c.apellidos, c.correo,
                c.puntos_acumulados, c.puntos_vencen_fecha, n.nivel_id, n.nombre_nivel, n.porcentaje_acumulacion_puntos,
                c.visitas_semestre_actual, siguiente.nombre_nivel AS siguiente_nivel, siguiente.visitas_min_semestre AS meta_visitas
            FROM public.cuenta_club c JOIN public.nivel_club n USING (nivel_id)
            LEFT JOIN LATERAL (SELECT nombre_nivel, visitas_min_semestre FROM public.nivel_club
                WHERE visitas_min_semestre > n.visitas_min_semestre ORDER BY visitas_min_semestre, nivel_id LIMIT 1) siguiente ON true
            WHERE c.socio_id = @SocioId
            """;
        await using var db = await factory.OpenAsync(token);
        return await db.QuerySingleOrDefaultAsync<PerfilClub>(new CommandDefinition(sql, new { socioId }, cancellationToken: token));
    }
}
