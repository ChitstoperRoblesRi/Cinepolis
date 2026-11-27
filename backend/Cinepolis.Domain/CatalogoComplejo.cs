namespace Cinepolis.Domain;

/// <summary>Entidad de catalogo complejo del sistema de cine.</summary>
public sealed class CatalogoComplejo
{
    public int CatalogoId { get; init; }

    public required string CatalogoNombre { get; init; }

    public int ComplejoId { get; init; }
}
