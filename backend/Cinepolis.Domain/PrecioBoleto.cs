namespace Cinepolis.Domain;

/// <summary>Entidad de precio boleto del sistema de cine.</summary>
public sealed class PrecioBoleto
{
    public int PrecioBoletoId { get; init; }

    public int ComplejoId { get; init; }

    public int FormatoId { get; init; }

    public int TipoBoletoId { get; init; }

    public required string TipoSala { get; init; }

    public decimal Precio { get; init; }

    public decimal CargoServicioOnline { get; init; }

    public string? AreaCategoryCode { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PrecioBoletoTipoSala TipoSalaTipado => ValoresPrecioBoletoTipoSala.DesdeTexto(TipoSala);
}
