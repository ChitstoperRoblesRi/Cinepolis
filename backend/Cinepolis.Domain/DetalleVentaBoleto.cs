using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_venta_boleto", Schema = "public")]
public sealed class DetalleVentaBoleto
{
    [Column("detalle_venta_boleto_id", TypeName = "integer")]
    public int DetalleVentaBoletoId { get; init; }

    [Column("tipo_boleto_id", TypeName = "integer")]
    public int TipoBoletoId { get; init; }

    [Column("precio_unitario", TypeName = "numeric(8,2)")]
    public decimal PrecioUnitario { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int VentaId { get; init; }

    [Column("asiento_id", TypeName = "integer")]
    public int AsientoId { get; init; }
}
