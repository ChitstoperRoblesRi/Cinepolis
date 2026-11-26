using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_venta_promocion", Schema = "public")]
public sealed class DetalleVentaPromocion
{
    [Column("detalle_venta_promocion_id", TypeName = "integer")]
    public int DetalleVentaPromocionId { get; init; }

    [Column("cantidad", TypeName = "integer")]
    public int Cantidad { get; init; }

    [Column("promocion_id", TypeName = "integer")]
    public int PromocionId { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int VentaId { get; init; }
}
