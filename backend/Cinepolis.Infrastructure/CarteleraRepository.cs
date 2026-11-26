using Cinepolis.Application;
using Cinepolis.Domain;
using Dapper;
using System.Text;

namespace Cinepolis.Infrastructure;

public sealed class CarteleraRepository(NpgsqlConnectionFactory factory) : ICarteleraRepository
{
    public async Task<FichaPelicula?> FichaAsync(int id, CancellationToken token)
    {
        const string sql = """
            SELECT p.pelicula_id, p.titulo, p.clasificacion, p.sinopsis, p.trailer_url, p.poster_url,
                   p.genero, p.duracion_min,
                   coalesce(array_agg(n.nombre ORDER BY n.nombre, n.persona_id)
                       FILTER (WHERE pp.rol = @Director), ARRAY[]::varchar[]) AS directores,
                   coalesce(array_agg(n.nombre ORDER BY n.nombre, n.persona_id)
                       FILTER (WHERE pp.rol = @Actor), ARRAY[]::varchar[]) AS reparto
            FROM public.pelicula p
            LEFT JOIN public.pelicula_persona pp ON pp.pelicula_id = p.pelicula_id
            LEFT JOIN public.persona n ON n.persona_id = pp.persona_id
            WHERE p.pelicula_id = @Id
            GROUP BY p.pelicula_id
            """;
        await using var db = await factory.OpenAsync(token);
        var parametros = new { Id = id, Director = PeliculaPersonaRol.Director.TextoSql(), Actor = PeliculaPersonaRol.Actor.TextoSql() };
        return await db.QuerySingleOrDefaultAsync<FichaPelicula>(new CommandDefinition(sql, parametros, cancellationToken: token));
    }

    public async Task<IReadOnlyList<FuncionCartelera>> FuncionesAsync(FiltrosCartelera filtros, int? peliculaId, CancellationToken token)
    {
        var sql = new StringBuilder("""
            SELECT p.pelicula_id, p.titulo, p.clasificacion, p.poster_url, s.tipo_sala, fs.nombre_formato,
                   f.fecha_funcion, f.hora_inicio, f.funcion_id, c.complejo_id, c.nombre_comercial, s.sala_id,
                   s.nombre_sala, f.idioma, f.hora_termino, f.es_preventa, p.genero, p.duracion_min,
                   p.categoria, p.banner_url, p.trailer_url
            FROM public.funcion f
            JOIN public.pelicula p ON p.pelicula_id = f.pelicula_id
            JOIN public.sala s ON s.sala_id = f.sala_id
            JOIN public.complejo c ON c.complejo_id = s.complejo_id
            JOIN public.formato_sala fs ON fs.formato_id = f.formato_id
            WHERE f.activa AND (f.fecha_funcion, f.hora_inicio) >= (CURRENT_DATE, LOCALTIME)
            """);
        if (peliculaId.HasValue) sql.Append(" AND f.pelicula_id = @PeliculaId");
        if (filtros.ComplejoId.HasValue) sql.Append(" AND s.complejo_id = @ComplejoId");
        if (filtros.Fecha.HasValue) sql.Append(" AND f.fecha_funcion = @Fecha");
        if (filtros.Formato is not null) sql.Append(" AND (lower(s.tipo_sala) = lower(@Formato) OR lower(fs.nombre_formato) = lower(@Formato))");
        if (filtros.Idioma is not null) sql.Append(" AND lower(f.idioma) = lower(@Idioma)");
        if (filtros.Clasificacion is not null) sql.Append(" AND lower(p.clasificacion) = lower(@Clasificacion)");
        sql.Append(" ORDER BY p.titulo, p.pelicula_id, s.tipo_sala, fs.nombre_formato, f.fecha_funcion, f.hora_inicio, f.funcion_id");

        await using var db = await factory.OpenAsync(token);
        var parametros = new { PeliculaId = peliculaId, filtros.ComplejoId, filtros.Fecha, filtros.Formato, filtros.Idioma, filtros.Clasificacion };
        return (await db.QueryAsync<FuncionCartelera>(new CommandDefinition(sql.ToString(), parametros,
            cancellationToken: token))).AsList();
    }
}
