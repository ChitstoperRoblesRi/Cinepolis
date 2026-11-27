namespace Cinepolis.Domain;

/// <summary>Entidad de tipo boleto del sistema de cine.</summary>
public sealed class TipoBoleto
{
    public int TipoBoletoId { get; init; }

    public required string Nombre { get; init; }

    public string? Descripcion { get; init; }

    public bool EsPromocional { get; init; }

    public decimal? PrecioFijo { get; init; }
}
