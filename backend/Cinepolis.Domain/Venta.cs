namespace Cinepolis.Domain;

/// <summary>Entidad de venta del sistema de cine.</summary>
public sealed class Venta
{
    public int VentaId { get; init; }

    public decimal ImporteTotal { get; init; }

    public decimal CargoServicio { get; init; }

    public required string FormaPago { get; init; }

    public required string TipoVenta { get; init; }

    public DateTime FechaVenta { get; init; }

    public int? SesionId { get; init; }

    public int? SocioId { get; init; }

    public string? NombreComprador { get; init; }

    public string? ApellidoComprador { get; init; }

    public string? CorreoComprador { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public VentaFormaPago FormaPagoTipado => ValoresVentaFormaPago.DesdeTexto(FormaPago);

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public VentaTipoVenta TipoVentaTipado => ValoresVentaTipoVenta.DesdeTexto(TipoVenta);
}
