namespace Cinepolis.Domain;

/// <summary>Entidad de rol del sistema de cine.</summary>
public sealed class Rol
{
    public int RolId { get; init; }

    public required string NombreRol { get; init; }

    public bool EsMando { get; init; }

    public string? DescripcionResponsabilidades { get; init; }

    public string? AreaTrabajo { get; init; }
}
