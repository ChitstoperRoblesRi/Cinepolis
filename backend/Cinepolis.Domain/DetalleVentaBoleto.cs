namespace Cinepolis.Domain;

/// <summary>Entidad de detalle venta boleto del sistema de cine.</summary>
public sealed class DetalleVentaBoleto
{
    public int DetalleVentaBoletoId { get; init; }

    public int TipoBoletoId { get; init; }

    public decimal PrecioUnitario { get; init; }

    public int VentaId { get; init; }

    public int AsientoId { get; init; }
}
