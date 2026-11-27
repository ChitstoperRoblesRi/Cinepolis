namespace Cinepolis.Domain;

/// <summary>Entidad de categoria dulceria del sistema de cine.</summary>
public sealed class CategoriaDulceria
{
    public int CategoriaId { get; init; }

    public required string NombreCategoria { get; init; }
}
