namespace Cinepolis.Domain;

/// <summary>Entidad de ciudad del sistema de cine.</summary>
public sealed class Ciudad
{
    public int CiudadId { get; init; }

    public required string NombreCiudad { get; init; }
}
