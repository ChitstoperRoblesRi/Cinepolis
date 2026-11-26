using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("punto_venta", Schema = "public")]
public sealed class PuntoVenta
{
    [Column("punto_venta_id", TypeName = "integer")]
    public int PuntoVentaId { get; init; }

    [Column("tipo_punto", TypeName = "character varying(10)")]
    public required string TipoPunto { get; init; }

    [Column("descripcion", TypeName = "character varying(200)")]
    public string? Descripcion { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [NotMapped]
    public PuntoVentaTipoPunto TipoPuntoTipado => ValoresPuntoVentaTipoPunto.DesdeTexto(TipoPunto);
}
