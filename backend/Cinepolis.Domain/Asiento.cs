namespace Cinepolis.Domain;

/// <summary>Entidad de asiento del sistema de cine.</summary>
public sealed class Asiento
{
    public int AsientoId { get; init; }

    public string? AsientoApiId { get; init; }

    public required string Fila { get; init; }

    public int Columna { get; init; }

    public required string TipoAsiento { get; init; }

    public required string Estado { get; init; }

    public int FuncionId { get; init; }

    public required string AreaCategoryCode { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public AsientoTipoAsiento TipoAsientoTipado => ValoresAsientoTipoAsiento.DesdeTexto(TipoAsiento);

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public AsientoEstado EstadoTipado => ValoresAsientoEstado.DesdeTexto(Estado);
}
