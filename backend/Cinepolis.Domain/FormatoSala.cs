namespace Cinepolis.Domain;

/// <summary>Entidad de formato sala del sistema de cine.</summary>
public sealed class FormatoSala
{
    public int FormatoId { get; init; }

    public required string NombreFormato { get; init; }

    public string? Descripcion { get; init; }

    public bool EsProyeccion { get; init; }
}
