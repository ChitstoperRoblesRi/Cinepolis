using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("categoria_dulceria", Schema = "public")]
public sealed class CategoriaDulceria
{
    [Column("categoria_id", TypeName = "integer")]
    public int CategoriaId { get; init; }

    [Column("nombre_categoria", TypeName = "character varying(100)")]
    public required string NombreCategoria { get; init; }
}
