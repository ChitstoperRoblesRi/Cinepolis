using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("garantia_reclamo", Schema = "public")]
public sealed class GarantiaReclamo
{
    [Column("reclamo_id", TypeName = "integer")]
    public int ReclamoId { get; init; }

    [Column("nombre_reclamante", TypeName = "character varying(150)")]
    public required string NombreReclamante { get; init; }

    [Column("correo", TypeName = "character varying(150)")]
    public required string Correo { get; init; }

    [Column("motivo", TypeName = "text")]
    public required string Motivo { get; init; }

    [Column("fecha_reclamo", TypeName = "timestamp without time zone")]
    public DateTime FechaReclamo { get; init; }

    [Column("estado_reclamo", TypeName = "character varying(15)")]
    public required string EstadoReclamo { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int? VentaId { get; init; }

    [Column("pelicula_id", TypeName = "integer")]
    public int PeliculaId { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [NotMapped]
    public GarantiaReclamoEstadoReclamo EstadoReclamoTipado => ValoresGarantiaReclamoEstadoReclamo.DesdeTexto(EstadoReclamo);
}
