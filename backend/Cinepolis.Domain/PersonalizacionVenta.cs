namespace Cinepolis.Domain;

/// <summary>Entidad de personalizacion venta del sistema de cine.</summary>
public sealed class PersonalizacionVenta
{
    public int PersonalizacionVentaId { get; init; }

    public int DetalleVentaProductoId { get; init; }

    public int OpcionId { get; init; }

    public int Cantidad { get; init; }

    public decimal? Porcentaje { get; init; }
}
