namespace Cinepolis.Domain;

/// <summary>Entidad de detalle venta producto del sistema de cine.</summary>
public sealed class DetalleVentaProducto
{
    public int DetalleProductoId { get; init; }

    public int Cantidad { get; init; }

    public decimal PrecioUnitario { get; init; }

    public int VentaId { get; init; }

    public int ProductoId { get; init; }
}
