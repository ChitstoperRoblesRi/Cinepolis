using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_catalogo", Schema = "public")]
public sealed class DetalleCatalogo
{
    [Column("detalle_id", TypeName = "integer")]
    public int DetalleId { get; init; }

    [Column("precio_actual", TypeName = "numeric(8,2)")]
    public decimal PrecioActual { get; init; }

    [Column("disponibilidad", TypeName = "boolean")]
    public bool Disponibilidad { get; init; }

    [Column("catalogo_id", TypeName = "integer")]
    public int CatalogoId { get; init; }

    [Column("producto_id", TypeName = "integer")]
    public int ProductoId { get; init; }
}
