namespace Cinepolis.Domain;

/// <summary>Entidad de historial producto del sistema de cine.</summary>
public sealed class HistorialProducto
{
    public int HistorialId { get; init; }

    public DateTime FechaCambio { get; init; }

    public decimal PrecioNuevo { get; init; }

    public decimal PrecioAnterior { get; init; }

    public int DetalleId { get; init; }
}
