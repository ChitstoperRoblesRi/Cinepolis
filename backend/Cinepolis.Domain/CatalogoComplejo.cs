using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("catalogo_complejo", Schema = "public")]
public sealed class CatalogoComplejo
{
    [Column("catalogo_id", TypeName = "integer")]
    public int CatalogoId { get; init; }

    [Column("catalogo_nombre", TypeName = "character varying(150)")]
    public required string CatalogoNombre { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }
}
