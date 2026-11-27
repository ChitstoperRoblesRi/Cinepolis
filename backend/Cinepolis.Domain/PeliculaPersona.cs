namespace Cinepolis.Domain;

/// <summary>Entidad de pelicula persona del sistema de cine.</summary>
public sealed class PeliculaPersona
{
    public int PeliculaId { get; init; }

    public int PersonaId { get; init; }

    public required string Rol { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PeliculaPersonaRol RolTipado => ValoresPeliculaPersonaRol.DesdeTexto(Rol);
}
