using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("nivel_club", Schema = "public")]
public sealed class NivelClub
{
    [Column("nivel_id", TypeName = "integer")]
    public int NivelId { get; init; }

    [Column("nombre_nivel", TypeName = "character varying(50)")]
    public required string NombreNivel { get; init; }

    [Column("visitas_min_semestre", TypeName = "integer")]
    public int VisitasMinSemestre { get; init; }

    [Column("visitas_max_semestre", TypeName = "integer")]
    public int? VisitasMaxSemestre { get; init; }

    [Column("porcentaje_acumulacion_puntos", TypeName = "numeric(5,2)")]
    public decimal PorcentajeAcumulacionPuntos { get; init; }

    [Column("descripcion_beneficios", TypeName = "text")]
    public string? DescripcionBeneficios { get; init; }
}
