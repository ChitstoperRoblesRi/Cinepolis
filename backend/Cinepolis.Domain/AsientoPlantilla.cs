using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("asiento_plantilla", Schema = "public")]
public sealed class AsientoPlantilla
{
    [Column("plantilla_id", TypeName = "integer")]
    public int PlantillaId { get; init; }

    [Column("asiento_api_id", TypeName = "character varying(100)")]
    public string? AsientoApiId { get; init; }

    [Column("sala_id", TypeName = "integer")]
    public int SalaId { get; init; }

    [Column("fila", TypeName = "character varying(5)")]
    public required string Fila { get; init; }

    [Column("columna", TypeName = "integer")]
    public int Columna { get; init; }

    [Column("tipo_asiento", TypeName = "character varying(20)")]
    public required string TipoAsiento { get; init; }

    [Column("area_category_code", TypeName = "character varying(20)")]
    public string? AreaCategoryCode { get; init; }

    [NotMapped]
    public AsientoTipoAsiento TipoAsientoTipado => ValoresAsientoTipoAsiento.DesdeTexto(TipoAsiento);
}
