namespace Cinepolis.Domain;

/// <summary>Entidad de detalle venta promocion del sistema de cine.</summary>
public sealed class DetalleVentaPromocion
{
    public int DetalleVentaPromocionId { get; init; }

    public int Cantidad { get; init; }

    public int PromocionId { get; init; }

    public int VentaId { get; init; }
}
