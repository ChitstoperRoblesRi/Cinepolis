namespace Cinepolis.Domain;

/// <summary>Entidad de producto dulceria del sistema de cine.</summary>
public sealed class ProductoDulceria
{
    public int ProductoId { get; init; }

    public required string ApiId { get; init; }

    public required string NombreProducto { get; init; }

    public required string Estructura { get; init; }

    public string? Descripcion { get; init; }

    public string? ImagenUrl { get; init; }

    public int CategoriaId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public ProductoDulceriaEstructura EstructuraTipado => ValoresProductoDulceriaEstructura.DesdeTexto(Estructura);
}
