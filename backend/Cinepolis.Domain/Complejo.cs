namespace Cinepolis.Domain;

/// <summary>Entidad de complejo del sistema de cine.</summary>
public sealed class Complejo
{
    public int ComplejoId { get; init; }

    public string? VistaId { get; init; }

    public required string NombreComercial { get; init; }

    public required string TipoComplejo { get; init; }

    public required string Direccion { get; init; }

    public required string CodigoPostal { get; init; }

    public DateOnly FechaApertura { get; init; }

    public required string Slug { get; init; }

    public string? ImagenUrl { get; init; }

    public int CiudadId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public ComplejoTipoComplejo TipoComplejoTipado => ValoresComplejoTipoComplejo.DesdeTexto(TipoComplejo);
}
