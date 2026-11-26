using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("asiento", Schema = "public")]
public sealed class Asiento
{
    [Column("asiento_id", TypeName = "integer")]
    public int AsientoId { get; init; }

    [Column("asiento_api_id", TypeName = "character varying(100)")]
    public string? AsientoApiId { get; init; }

    [Column("fila", TypeName = "character varying(5)")]
    public required string Fila { get; init; }

    [Column("columna", TypeName = "integer")]
    public int Columna { get; init; }

    [Column("tipo_asiento", TypeName = "character varying(20)")]
    public required string TipoAsiento { get; init; }

    [Column("estado", TypeName = "character varying(15)")]
    public required string Estado { get; init; }

    [Column("funcion_id", TypeName = "integer")]
    public int FuncionId { get; init; }

    [Column("area_category_code", TypeName = "character varying(20)")]
    public required string AreaCategoryCode { get; init; }

    [NotMapped]
    public AsientoTipoAsiento TipoAsientoTipado => ValoresAsientoTipoAsiento.DesdeTexto(TipoAsiento);

    [NotMapped]
    public AsientoEstado EstadoTipado => ValoresAsientoEstado.DesdeTexto(Estado);
}
