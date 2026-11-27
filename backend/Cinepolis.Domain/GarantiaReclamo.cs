namespace Cinepolis.Domain;

/// <summary>Entidad de garantia reclamo del sistema de cine.</summary>
public sealed class GarantiaReclamo
{
    public int ReclamoId { get; init; }

    public required string NombreReclamante { get; init; }

    public required string Correo { get; init; }

    public required string Motivo { get; init; }

    public DateTime FechaReclamo { get; init; }

    public required string EstadoReclamo { get; init; }

    public int? VentaId { get; init; }

    public int PeliculaId { get; init; }

    public int ComplejoId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public GarantiaReclamoEstadoReclamo EstadoReclamoTipado => ValoresGarantiaReclamoEstadoReclamo.DesdeTexto(EstadoReclamo);
}
