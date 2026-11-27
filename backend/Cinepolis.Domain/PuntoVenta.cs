namespace Cinepolis.Domain;

/// <summary>Entidad de punto venta del sistema de cine.</summary>
public sealed class PuntoVenta
{
    public int PuntoVentaId { get; init; }

    public required string TipoPunto { get; init; }

    public string? Descripcion { get; init; }

    public int ComplejoId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PuntoVentaTipoPunto TipoPuntoTipado => ValoresPuntoVentaTipoPunto.DesdeTexto(TipoPunto);
}
