using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("ciudad", Schema = "public")]
public sealed class Ciudad
{
    [Column("ciudad_id", TypeName = "integer")]
    public int CiudadId { get; init; }

    [Column("nombre_ciudad", TypeName = "character varying(100)")]
    public required string NombreCiudad { get; init; }
}
