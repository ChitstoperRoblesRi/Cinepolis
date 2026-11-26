using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("funcion", Schema = "public")]
public sealed class Funcion
{
    [Column("funcion_id", TypeName = "integer")]
    public int FuncionId { get; init; }

    [Column("fecha_funcion", TypeName = "date")]
    public DateOnly FechaFuncion { get; init; }

    [Column("hora_inicio", TypeName = "time without time zone")]
    public TimeOnly HoraInicio { get; init; }

    [Column("hora_termino", TypeName = "time without time zone")]
    public TimeOnly HoraTermino { get; init; }

    [Column("idioma", TypeName = "character varying(15)")]
    public required string Idioma { get; init; }

    [Column("es_preventa", TypeName = "boolean")]
    public bool EsPreventa { get; init; }

    [Column("formato_id", TypeName = "integer")]
    public int FormatoId { get; init; }

    [Column("sala_id", TypeName = "integer")]
    public int SalaId { get; init; }

    [Column("pelicula_id", TypeName = "integer")]
    public int PeliculaId { get; init; }

    [Column("activa", TypeName = "boolean")]
    public bool Activa { get; init; }

    [NotMapped]
    public FuncionIdioma IdiomaTipado => ValoresFuncionIdioma.DesdeTexto(Idioma);
}
