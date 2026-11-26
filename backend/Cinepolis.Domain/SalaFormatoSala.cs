using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("sala_formato_sala", Schema = "public")]
public sealed class SalaFormatoSala
{
    [Column("sala_id", TypeName = "integer")]
    public int SalaId { get; init; }

    [Column("formato_id", TypeName = "integer")]
    public int FormatoId { get; init; }
}
