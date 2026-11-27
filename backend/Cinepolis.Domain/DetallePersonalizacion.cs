namespace Cinepolis.Domain;

/// <summary>Entidad de detalle personalizacion del sistema de cine.</summary>
public sealed class DetallePersonalizacion
{
    public int PersonalizacionId { get; init; }

    public int DetalleCarritoId { get; init; }

    public int OpcionId { get; init; }

    public int Cantidad { get; init; }

    public decimal? Porcentaje { get; init; }
}
