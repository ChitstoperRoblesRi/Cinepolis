namespace Cinepolis.Domain;

/// <summary>Entidad de disponibilidad modificador del sistema de cine.</summary>
public sealed class DisponibilidadModificador
{
    public int DispId { get; init; }

    public int OpcionId { get; init; }

    public int CatalogoId { get; init; }

    public bool Activo { get; init; }

    public decimal PrecioExtra { get; init; }

    public DateTime ActualizadoEn { get; init; }
}
