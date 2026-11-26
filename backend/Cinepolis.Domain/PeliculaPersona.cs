using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("pelicula_persona", Schema = "public")]
public sealed class PeliculaPersona
{
    [Column("pelicula_id", TypeName = "integer")]
    public int PeliculaId { get; init; }

    [Column("persona_id", TypeName = "integer")]
    public int PersonaId { get; init; }

    [Column("rol", TypeName = "character varying(10)")]
    public required string Rol { get; init; }

    [NotMapped]
    public PeliculaPersonaRol RolTipado => ValoresPeliculaPersonaRol.DesdeTexto(Rol);
}
