using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("tipo_boleto", Schema = "public")]
public sealed class TipoBoleto
{
    [Column("tipo_boleto_id", TypeName = "integer")]
    public int TipoBoletoId { get; init; }

    [Column("nombre", TypeName = "character varying(100)")]
    public required string Nombre { get; init; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; init; }

    [Column("es_promocional", TypeName = "boolean")]
    public bool EsPromocional { get; init; }

    [Column("precio_fijo", TypeName = "numeric(8,2)")]
    public decimal? PrecioFijo { get; init; }
}
