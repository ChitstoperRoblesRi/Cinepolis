using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("promocion_complejo", Schema = "public")]
public sealed class PromocionComplejo
{
    [Column("promocion_id", TypeName = "integer")]
    public int PromocionId { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }
}
