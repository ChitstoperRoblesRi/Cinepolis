namespace Cinepolis.Domain;

/// <summary>Entidad de promocion del sistema de cine.</summary>
public sealed class Promocion
{
    public int PromocionId { get; init; }

    public required string NombrePromocion { get; init; }

    public required string TipoPromocion { get; init; }

    public bool AplicaTodos { get; init; }

    public string? Restricciones { get; init; }

    public DateOnly FechaInicioVigencia { get; init; }

    public DateOnly? FechaFinVigencia { get; init; }

    public string? Descripcion { get; init; }

    public int? NivelId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PromocionTipoPromocion TipoPromocionTipado => ValoresPromocionTipoPromocion.DesdeTexto(TipoPromocion);
}
