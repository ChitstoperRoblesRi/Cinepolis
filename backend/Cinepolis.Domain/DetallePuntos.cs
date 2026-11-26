using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_puntos", Schema = "public")]
public sealed class DetallePuntos
{
    [Column("detalle_puntos_id", TypeName = "integer")]
    public int DetallePuntosId { get; init; }

    [Column("fecha_movimiento", TypeName = "timestamp without time zone")]
    public DateTime FechaMovimiento { get; init; }

    [Column("tipo_movimiento", TypeName = "character varying(15)")]
    public required string TipoMovimiento { get; init; }

    [Column("cantidad_puntos", TypeName = "numeric(10,2)")]
    public decimal CantidadPuntos { get; init; }

    [Column("descripcion_movimiento", TypeName = "character varying(200)")]
    public string? DescripcionMovimiento { get; init; }

    [Column("socio_id", TypeName = "integer")]
    public int SocioId { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int? VentaId { get; init; }

    [NotMapped]
    public DetallePuntosTipoMovimiento TipoMovimientoTipado => ValoresDetallePuntosTipoMovimiento.DesdeTexto(TipoMovimiento);
}
