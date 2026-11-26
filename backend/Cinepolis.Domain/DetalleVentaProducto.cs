using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_venta_producto", Schema = "public")]
public sealed class DetalleVentaProducto
{
    [Column("detalle_producto_id", TypeName = "integer")]
    public int DetalleProductoId { get; init; }

    [Column("cantidad", TypeName = "integer")]
    public int Cantidad { get; init; }

    [Column("precio_unitario", TypeName = "numeric(8,2)")]
    public decimal PrecioUnitario { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int VentaId { get; init; }

    [Column("producto_id", TypeName = "integer")]
    public int ProductoId { get; init; }
}
