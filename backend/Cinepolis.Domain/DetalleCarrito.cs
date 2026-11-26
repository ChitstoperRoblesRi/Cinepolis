using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_carrito", Schema = "public")]
public sealed class DetalleCarrito
{
    [Column("detalle_carrito_id", TypeName = "integer")]
    public int DetalleCarritoId { get; init; }

    [Column("carrito_id", TypeName = "integer")]
    public int CarritoId { get; init; }

    [Column("tipo_item", TypeName = "character varying(10)")]
    public required string TipoItem { get; init; }

    [Column("cantidad", TypeName = "integer")]
    public int Cantidad { get; init; }

    [Column("precio_unitario", TypeName = "numeric(8,2)")]
    public decimal PrecioUnitario { get; init; }

    [Column("asiento_id", TypeName = "integer")]
    public int? AsientoId { get; init; }

    [Column("producto_id", TypeName = "integer")]
    public int? ProductoId { get; init; }

    [Column("promocion_id", TypeName = "integer")]
    public int? PromocionId { get; init; }

    [Column("tipo_boleto_id", TypeName = "integer")]
    public int? TipoBoletoId { get; init; }

    [NotMapped]
    public DetalleCarritoTipoItem TipoItemTipado => ValoresDetalleCarritoTipoItem.DesdeTexto(TipoItem);
}
