using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("formato_sala", Schema = "public")]
public sealed class FormatoSala
{
    [Column("formato_id", TypeName = "integer")]
    public int FormatoId { get; init; }

    [Column("nombre_formato", TypeName = "character varying(50)")]
    public required string NombreFormato { get; init; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; init; }

    [Column("es_proyeccion", TypeName = "boolean")]
    public bool EsProyeccion { get; init; }
}
