namespace Cinepolis.Domain;

/// <summary>Entidad de detalle puntos del sistema de cine.</summary>
public sealed class DetallePuntos
{
    public int DetallePuntosId { get; init; }

    public DateTime FechaMovimiento { get; init; }

    public required string TipoMovimiento { get; init; }

    public decimal CantidadPuntos { get; init; }

    public string? DescripcionMovimiento { get; init; }

    public int SocioId { get; init; }

    public int? VentaId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public DetallePuntosTipoMovimiento TipoMovimientoTipado => ValoresDetallePuntosTipoMovimiento.DesdeTexto(TipoMovimiento);
}
