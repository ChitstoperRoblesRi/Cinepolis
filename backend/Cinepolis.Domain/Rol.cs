using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("rol", Schema = "public")]
public sealed class Rol
{
    [Column("rol_id", TypeName = "integer")]
    public int RolId { get; init; }

    [Column("nombre_rol", TypeName = "character varying(100)")]
    public required string NombreRol { get; init; }

    [Column("es_mando", TypeName = "boolean")]
    public bool EsMando { get; init; }

    [Column("descripcion_responsabilidades", TypeName = "text")]
    public string? DescripcionResponsabilidades { get; init; }

    [Column("area_trabajo", TypeName = "character varying(100)")]
    public string? AreaTrabajo { get; init; }
}
