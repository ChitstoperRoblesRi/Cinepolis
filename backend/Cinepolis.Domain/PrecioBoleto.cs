using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("precio_boleto", Schema = "public")]
public sealed class PrecioBoleto
{
    [Column("precio_boleto_id", TypeName = "integer")]
    public int PrecioBoletoId { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [Column("formato_id", TypeName = "integer")]
    public int FormatoId { get; init; }

    [Column("tipo_boleto_id", TypeName = "integer")]
    public int TipoBoletoId { get; init; }

    [Column("tipo_sala", TypeName = "character varying(15)")]
    public required string TipoSala { get; init; }

    [Column("precio", TypeName = "numeric(8,2)")]
    public decimal Precio { get; init; }

    [Column("cargo_servicio_online", TypeName = "numeric(8,2)")]
    public decimal CargoServicioOnline { get; init; }

    [Column("area_category_code", TypeName = "character varying(20)")]
    public string? AreaCategoryCode { get; init; }

    [NotMapped]
    public PrecioBoletoTipoSala TipoSalaTipado => ValoresPrecioBoletoTipoSala.DesdeTexto(TipoSala);
}
