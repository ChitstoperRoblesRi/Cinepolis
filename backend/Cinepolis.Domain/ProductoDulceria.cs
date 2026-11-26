using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("producto_dulceria", Schema = "public")]
public sealed class ProductoDulceria
{
    [Column("producto_id", TypeName = "integer")]
    public int ProductoId { get; init; }

    [Column("api_id", TypeName = "character varying(50)")]
    public required string ApiId { get; init; }

    [Column("nombre_producto", TypeName = "character varying(150)")]
    public required string NombreProducto { get; init; }

    [Column("estructura", TypeName = "character varying(20)")]
    public required string Estructura { get; init; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; init; }

    [Column("imagen_url", TypeName = "character varying(500)")]
    public string? ImagenUrl { get; init; }

    [Column("categoria_id", TypeName = "integer")]
    public int CategoriaId { get; init; }

    [NotMapped]
    public ProductoDulceriaEstructura EstructuraTipado => ValoresProductoDulceriaEstructura.DesdeTexto(Estructura);
}
