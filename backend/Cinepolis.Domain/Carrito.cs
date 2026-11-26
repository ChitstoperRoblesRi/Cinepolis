using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("carrito", Schema = "public")]
public sealed class Carrito
{
    [Column("carrito_id", TypeName = "integer")]
    public int CarritoId { get; init; }

    [Column("uuid_sesion_web", TypeName = "character varying(100)")]
    public string? UuidSesionWeb { get; init; }

    [Column("fecha_creacion", TypeName = "timestamp without time zone")]
    public DateTime FechaCreacion { get; init; }

    [Column("fecha_expiracion", TypeName = "timestamp without time zone")]
    public DateTime FechaExpiracion { get; init; }

    [Column("estado", TypeName = "character varying(15)")]
    public required string Estado { get; init; }

    [Column("sesion_id", TypeName = "integer")]
    public int? SesionId { get; init; }

    [Column("socio_id", TypeName = "integer")]
    public int? SocioId { get; init; }

    [NotMapped]
    public CarritoEstado EstadoTipado => ValoresCarritoEstado.DesdeTexto(Estado);
}
