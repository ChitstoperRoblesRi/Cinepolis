namespace Cinepolis.Domain;

/// <summary>Entidad de modificador opcion del sistema de cine.</summary>
public sealed class ModificadorOpcion
{
    public int OpcionId { get; init; }

    public int ReglaId { get; init; }

    public required string ApiIdOpcion { get; init; }

    public required string NombreOpcion { get; init; }

    public decimal PrecioAdicional { get; init; }

    public string? ImagenUrl { get; init; }

    public bool? EsDefault { get; init; }
}
