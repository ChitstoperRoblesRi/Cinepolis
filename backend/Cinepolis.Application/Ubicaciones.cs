namespace Cinepolis.Application;

public sealed record CiudadDto(int CiudadId, string NombreCiudad);
public sealed record ComplejoDto(int ComplejoId, int CiudadId, string NombreComercial, string TipoComplejo,
    string Direccion, string CodigoPostal, string Slug, string? ImagenUrl);

public interface IUbicacionesRepository
{
    Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken token);
    Task<IReadOnlyList<ComplejoDto>> ComplejosAsync(int? ciudadId, CancellationToken token);
}

public sealed class ConsultarUbicaciones(IUbicacionesRepository repository)
{
    public Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken token) => repository.CiudadesAsync(token);
    public Task<IReadOnlyList<ComplejoDto>> ComplejosAsync(int? ciudadId, CancellationToken token) =>
        repository.ComplejosAsync(ciudadId, token);
}
