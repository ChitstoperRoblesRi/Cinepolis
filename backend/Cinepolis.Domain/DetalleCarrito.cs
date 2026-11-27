namespace Cinepolis.Domain;

/// <summary>Entidad de detalle carrito del sistema de cine.</summary>
public sealed class DetalleCarrito
{
    public int DetalleCarritoId { get; init; }

    public int CarritoId { get; init; }

    public required string TipoItem { get; init; }

    public int Cantidad { get; init; }

    public decimal PrecioUnitario { get; init; }

    public int? AsientoId { get; init; }

    public int? ProductoId { get; init; }

    public int? PromocionId { get; init; }

    public int? TipoBoletoId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public DetalleCarritoTipoItem TipoItemTipado => ValoresDetalleCarritoTipoItem.DesdeTexto(TipoItem);
}
