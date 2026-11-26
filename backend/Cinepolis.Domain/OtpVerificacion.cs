using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("otp_verificacion", Schema = "public")]
public sealed class OtpVerificacion
{
    [Column("otp_id", TypeName = "integer")]
    public int OtpId { get; init; }

    [Column("correo", TypeName = "character varying(150)")]
    public required string Correo { get; init; }

    [Column("codigo", TypeName = "character varying(6)")]
    public required string Codigo { get; init; }

    [Column("intentos", TypeName = "integer")]
    public int Intentos { get; init; }

    [Column("expiracion", TypeName = "timestamp without time zone")]
    public DateTime Expiracion { get; init; }

    [Column("usado", TypeName = "boolean")]
    public bool Usado { get; init; }
}
