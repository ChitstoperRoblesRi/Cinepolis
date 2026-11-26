using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("sala", Schema = "public")]
public sealed class Sala
{
    [Column("sala_id", TypeName = "integer")]
    public int SalaId { get; init; }

    [Column("numero_sala", TypeName = "integer")]
    public int NumeroSala { get; init; }

    [Column("nombre_sala", TypeName = "character varying(50)")]
    public required string NombreSala { get; init; }

    [Column("capacidad_asientos", TypeName = "integer")]
    public int CapacidadAsientos { get; init; }

    [Column("tipo_sala", TypeName = "character varying(15)")]
    public required string TipoSala { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [NotMapped]
    public PrecioBoletoTipoSala TipoSalaTipado => ValoresPrecioBoletoTipoSala.DesdeTexto(TipoSala);
}
