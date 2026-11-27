namespace Cinepolis.Domain;

/// <summary>Entidad de sesion del sistema de cine.</summary>
public sealed class Sesion
{
    public int SesionId { get; init; }

    public required string UsuarioEmpleado { get; init; }

    public required string EstadoSesion { get; init; }

    public decimal MontoInicial { get; init; }

    public decimal? MontoFinalSistema { get; init; }

    public DateTime FechaHoraInicio { get; init; }

    public DateTime? FechaHoraFin { get; init; }

    public int EmpleadoId { get; init; }

    public int PuntoVentaId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public SesionEstadoSesion EstadoSesionTipado => ValoresSesionEstadoSesion.DesdeTexto(EstadoSesion);
}
