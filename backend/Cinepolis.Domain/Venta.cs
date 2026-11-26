using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("venta", Schema = "public")]
public sealed class Venta
{
    [Column("venta_id", TypeName = "integer")]
    public int VentaId { get; init; }

    [Column("importe_total", TypeName = "numeric(10,2)")]
    public decimal ImporteTotal { get; init; }

    [Column("cargo_servicio", TypeName = "numeric(8,2)")]
    public decimal CargoServicio { get; init; }

    [Column("forma_pago", TypeName = "character varying(15)")]
    public required string FormaPago { get; init; }

    [Column("tipo_venta", TypeName = "character varying(10)")]
    public required string TipoVenta { get; init; }

    [Column("fecha_venta", TypeName = "timestamp without time zone")]
    public DateTime FechaVenta { get; init; }

    [Column("sesion_id", TypeName = "integer")]
    public int? SesionId { get; init; }

    [Column("socio_id", TypeName = "integer")]
    public int? SocioId { get; init; }

    [Column("nombre_comprador", TypeName = "character varying(100)")]
    public string? NombreComprador { get; init; }

    [Column("apellido_comprador", TypeName = "character varying(100)")]
    public string? ApellidoComprador { get; init; }

    [Column("correo_comprador", TypeName = "character varying(150)")]
    public string? CorreoComprador { get; init; }

    [NotMapped]
    public VentaFormaPago FormaPagoTipado => ValoresVentaFormaPago.DesdeTexto(FormaPago);

    [NotMapped]
    public VentaTipoVenta TipoVentaTipado => ValoresVentaTipoVenta.DesdeTexto(TipoVenta);
}
