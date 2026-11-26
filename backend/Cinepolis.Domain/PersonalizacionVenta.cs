using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("personalizacion_venta", Schema = "public")]
public sealed class PersonalizacionVenta
{
    [Column("personalizacion_venta_id", TypeName = "integer")]
    public int PersonalizacionVentaId { get; init; }

    [Column("detalle_venta_producto_id", TypeName = "integer")]
    public int DetalleVentaProductoId { get; init; }

    [Column("opcion_id", TypeName = "integer")]
    public int OpcionId { get; init; }

    [Column("cantidad", TypeName = "integer")]
    public int Cantidad { get; init; }

    [Column("porcentaje", TypeName = "numeric(5,2)")]
    public decimal? Porcentaje { get; init; }
}
