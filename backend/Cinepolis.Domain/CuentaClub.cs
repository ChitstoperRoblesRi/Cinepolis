using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("cuenta_club", Schema = "public")]
public sealed class CuentaClub
{
    [Column("socio_id", TypeName = "integer")]
    public int SocioId { get; init; }

    [Column("usuario_cc", TypeName = "character varying(100)")]
    public required string UsuarioCc { get; init; }

    [Column("numero_tarjeta", TypeName = "character varying(20)")]
    public required string NumeroTarjeta { get; init; }

    [Column("nombre", TypeName = "character varying(100)")]
    public required string Nombre { get; init; }

    [Column("apellidos", TypeName = "character varying(150)")]
    public required string Apellidos { get; init; }

    [Column("correo", TypeName = "character varying(150)")]
    public required string Correo { get; init; }

    [Column("telefono", TypeName = "character varying(20)")]
    public string? Telefono { get; init; }

    [Column("fecha_nacimiento", TypeName = "date")]
    public DateOnly FechaNacimiento { get; init; }

    [Column("genero", TypeName = "character varying(30)")]
    public string? Genero { get; init; }

    [Column("codigo_postal", TypeName = "character varying(10)")]
    public string? CodigoPostal { get; init; }

    [Column("fecha_registro", TypeName = "date")]
    public DateOnly FechaRegistro { get; init; }

    [Column("puntos_acumulados", TypeName = "integer")]
    public int PuntosAcumulados { get; init; }

    [Column("puntos_vencen_fecha", TypeName = "date")]
    public DateOnly PuntosVencenFecha { get; init; }

    [Column("visitas_semestre_actual", TypeName = "integer")]
    public int VisitasSemestreActual { get; init; }

    [Column("semestre_actual", TypeName = "character varying(20)")]
    public required string SemestreActual { get; init; }

    [Column("nivel_id", TypeName = "integer")]
    public int NivelId { get; init; }

    [NotMapped]
    public CuentaClubGenero? GeneroTipado => Genero is null ? null : ValoresCuentaClubGenero.DesdeTexto(Genero);
}
