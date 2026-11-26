using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("persona", Schema = "public")]
public sealed class Persona
{
    [Column("persona_id", TypeName = "integer")]
    public int PersonaId { get; init; }

    [Column("nombre", TypeName = "character varying(150)")]
    public required string Nombre { get; init; }
}
