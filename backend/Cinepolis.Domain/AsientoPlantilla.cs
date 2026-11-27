namespace Cinepolis.Domain;

/// <summary>Entidad de asiento plantilla del sistema de cine.</summary>
public sealed class AsientoPlantilla
{
    public int PlantillaId { get; init; }

    public string? AsientoApiId { get; init; }

    public int SalaId { get; init; }

    public required string Fila { get; init; }

    public int Columna { get; init; }

    public required string TipoAsiento { get; init; }

    public string? AreaCategoryCode { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public AsientoTipoAsiento TipoAsientoTipado => ValoresAsientoTipoAsiento.DesdeTexto(TipoAsiento);
}
