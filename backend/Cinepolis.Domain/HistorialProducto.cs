using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("historial_producto", Schema = "public")]
public sealed class HistorialProducto
{
    [Column("historial_id", TypeName = "integer")]
    public int HistorialId { get; init; }

    [Column("fecha_cambio", TypeName = "timestamp without time zone")]
    public DateTime FechaCambio { get; init; }

    [Column("precio_nuevo", TypeName = "numeric(8,2)")]
    public decimal PrecioNuevo { get; init; }

    [Column("precio_anterior", TypeName = "numeric(8,2)")]
    public decimal PrecioAnterior { get; init; }

    [Column("detalle_id", TypeName = "integer")]
    public int DetalleId { get; init; }
}
