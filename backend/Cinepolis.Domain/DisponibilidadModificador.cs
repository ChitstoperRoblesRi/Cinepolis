using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("disponibilidad_modificador", Schema = "public")]
public sealed class DisponibilidadModificador
{
    [Column("disp_id", TypeName = "integer")]
    public int DispId { get; init; }

    [Column("opcion_id", TypeName = "integer")]
    public int OpcionId { get; init; }

    [Column("catalogo_id", TypeName = "integer")]
    public int CatalogoId { get; init; }

    [Column("activo", TypeName = "boolean")]
    public bool Activo { get; init; }

    [Column("precio_extra", TypeName = "numeric(10,2)")]
    public decimal PrecioExtra { get; init; }

    [Column("actualizado_en", TypeName = "timestamp with time zone")]
    public DateTime ActualizadoEn { get; init; }
}
