namespace Cinepolis.Application;

/// <summary>Ubicación comercial disponible para la selección de cine.</summary>
public sealed record CiudadDto(int CiudadId, string NombreCiudad);
/// <summary>Ubicación comercial disponible para la selección de cine.</summary>
public sealed record ComplejoDto(int ComplejoId, int CiudadId, string NombreComercial, string TipoComplejo,
    string Direccion, string CodigoPostal, string Slug, string? ImagenUrl);

/// <summary>Consulta ciudades y complejos disponibles.</summary>
public interface IComplejoQueryRepository
{
    /// <summary>Lista las ciudades con presencia comercial.</summary>
    Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken token);
    /// <summary>Lista los complejos según la ciudad solicitada.</summary>
    Task<IReadOnlyList<ComplejoDto>> ComplejosAsync(int? ciudadId, CancellationToken token);
}

/// <summary>Consulta ciudades y complejos disponibles.</summary>
public sealed class ConsultarUbicaciones(IComplejoQueryRepository repository)
{
    /// <summary>Lista las ciudades con presencia comercial.</summary>
    public Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken token) => repository.CiudadesAsync(token);
    /// <summary>Lista los complejos según la ciudad solicitada.</summary>
    public Task<IReadOnlyList<ComplejoDto>> ComplejosAsync(int? ciudadId, CancellationToken token) =>
        repository.ComplejosAsync(ciudadId, token);
}
