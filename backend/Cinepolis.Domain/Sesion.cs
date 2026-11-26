using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("sesion", Schema = "public")]
public sealed class Sesion
{
    [Column("sesion_id", TypeName = "integer")]
    public int SesionId { get; init; }

    [Column("usuario_empleado", TypeName = "character varying(100)")]
    public required string UsuarioEmpleado { get; init; }

    [Column("estado_sesion", TypeName = "character varying(10)")]
    public required string EstadoSesion { get; init; }

    [Column("monto_inicial", TypeName = "numeric(10,2)")]
    public decimal MontoInicial { get; init; }

    [Column("monto_final_sistema", TypeName = "numeric(10,2)")]
    public decimal? MontoFinalSistema { get; init; }

    [Column("fecha_hora_inicio", TypeName = "timestamp without time zone")]
    public DateTime FechaHoraInicio { get; init; }

    [Column("fecha_hora_fin", TypeName = "timestamp without time zone")]
    public DateTime? FechaHoraFin { get; init; }

    [Column("empleado_id", TypeName = "integer")]
    public int EmpleadoId { get; init; }

    [Column("punto_venta_id", TypeName = "integer")]
    public int PuntoVentaId { get; init; }

    [NotMapped]
    public SesionEstadoSesion EstadoSesionTipado => ValoresSesionEstadoSesion.DesdeTexto(EstadoSesion);
}
