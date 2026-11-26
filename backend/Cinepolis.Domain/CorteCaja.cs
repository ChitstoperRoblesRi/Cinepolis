using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("corte_caja", Schema = "public")]
public sealed class CorteCaja
{
    [Column("corte_id", TypeName = "integer")]
    public int CorteId { get; init; }

    [Column("monto_efectivo_contado", TypeName = "numeric(10,2)")]
    public decimal MontoEfectivoContado { get; init; }

    [Column("monto_reportado_sistema", TypeName = "numeric(10,2)")]
    public decimal MontoReportadoSistema { get; init; }

    [Column("diferencia", TypeName = "numeric(10,2)")]
    public decimal Diferencia { get; init; }

    [Column("fecha_hora_cierre", TypeName = "timestamp without time zone")]
    public DateTime FechaHoraCierre { get; init; }

    [Column("sesion_id", TypeName = "integer")]
    public int SesionId { get; init; }
}
