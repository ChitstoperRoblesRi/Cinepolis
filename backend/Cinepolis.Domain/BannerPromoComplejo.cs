using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("banner_promo_complejo", Schema = "public")]
public sealed class BannerPromoComplejo
{
    [Column("banner_id", TypeName = "integer")]
    public int BannerId { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }
}
