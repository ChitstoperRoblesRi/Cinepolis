namespace Cinepolis.Domain;

/// <summary>Entidad de persona del sistema de cine.</summary>
public sealed class Persona
{
    public int PersonaId { get; init; }

    public required string Nombre { get; init; }
}
