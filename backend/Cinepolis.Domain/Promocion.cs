using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("promocion", Schema = "public")]
public sealed class Promocion
{
    [Column("promocion_id", TypeName = "integer")]
    public int PromocionId { get; init; }

    [Column("nombre_promocion", TypeName = "character varying(150)")]
    public required string NombrePromocion { get; init; }

    [Column("tipo_promocion", TypeName = "character varying(20)")]
    public required string TipoPromocion { get; init; }

    [Column("aplica_todos", TypeName = "boolean")]
    public bool AplicaTodos { get; init; }

    [Column("restricciones", TypeName = "text")]
    public string? Restricciones { get; init; }

    [Column("fecha_inicio_vigencia", TypeName = "date")]
    public DateOnly FechaInicioVigencia { get; init; }

    [Column("fecha_fin_vigencia", TypeName = "date")]
    public DateOnly? FechaFinVigencia { get; init; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; init; }

    [Column("nivel_id", TypeName = "integer")]
    public int? NivelId { get; init; }

    [NotMapped]
    public PromocionTipoPromocion TipoPromocionTipado => ValoresPromocionTipoPromocion.DesdeTexto(TipoPromocion);
}
