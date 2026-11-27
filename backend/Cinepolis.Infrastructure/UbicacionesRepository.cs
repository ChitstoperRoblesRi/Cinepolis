using Cinepolis.Application;
using Dapper;

namespace Cinepolis.Infrastructure;

/// <summary>Consulta ciudades y complejos disponibles.</summary>
public sealed class UbicacionesRepository(IDbConnectionFactory factory) : IComplejoQueryRepository
{
    /// <summary>Lista las ciudades con presencia comercial.</summary>
    public async Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken token)
    {
        const string sql = """
            SELECT ciudad_id, nombre_ciudad
            FROM public.ciudad
            ORDER BY nombre_ciudad, ciudad_id
            """;
        await using var db = await factory.OpenAsync(token);
        return (await db.QueryAsync<CiudadDto>(new CommandDefinition(sql, cancellationToken: token))).AsList();
    }

    /// <summary>Lista los complejos según la ciudad solicitada.</summary>
    public async Task<IReadOnlyList<ComplejoDto>> ComplejosAsync(int? ciudadId, CancellationToken token)
    {
        const string sql = """
            SELECT complejo_id, ciudad_id, nombre_comercial, tipo_complejo, direccion, codigo_postal, slug, imagen_url
            FROM public.complejo
            WHERE fecha_apertura <= CURRENT_DATE
            """;
        var consulta = sql + (ciudadId.HasValue ? " AND ciudad_id = @CiudadId" : "")
            + " ORDER BY nombre_comercial, complejo_id";
        await using var db = await factory.OpenAsync(token);
        return (await db.QueryAsync<ComplejoDto>(new CommandDefinition(consulta, new { CiudadId = ciudadId },
            cancellationToken: token))).AsList();
    }
}
