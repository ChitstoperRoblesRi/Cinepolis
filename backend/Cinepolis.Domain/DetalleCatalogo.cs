namespace Cinepolis.Domain;

/// <summary>Entidad de detalle catalogo del sistema de cine.</summary>
public sealed class DetalleCatalogo
{
    public int DetalleId { get; init; }

    public decimal PrecioActual { get; init; }

    public bool Disponibilidad { get; init; }

    public int CatalogoId { get; init; }

    public int ProductoId { get; init; }
}
