using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("empleado", Schema = "public")]
public sealed class Empleado
{
    [Column("empleado_id", TypeName = "integer")]
    public int EmpleadoId { get; init; }

    [Column("nombre", TypeName = "character varying(100)")]
    public required string Nombre { get; init; }

    [Column("apellido_paterno", TypeName = "character varying(100)")]
    public required string ApellidoPaterno { get; init; }

    [Column("apellido_materno", TypeName = "character varying(100)")]
    public string? ApellidoMaterno { get; init; }

    [Column("telefono", TypeName = "character varying(20)")]
    public string? Telefono { get; init; }

    [Column("salario", TypeName = "numeric(10,2)")]
    public decimal Salario { get; init; }

    [Column("fecha_nacimiento", TypeName = "date")]
    public DateOnly FechaNacimiento { get; init; }

    [Column("estatus", TypeName = "character varying(10)")]
    public required string Estatus { get; init; }

    [Column("fecha_contratacion", TypeName = "date")]
    public DateOnly FechaContratacion { get; init; }

    [Column("correo", TypeName = "character varying(150)")]
    public string? Correo { get; init; }

    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [Column("rol_id", TypeName = "integer")]
    public int RolId { get; init; }

    [NotMapped]
    public EmpleadoEstatus EstatusTipado => ValoresEmpleadoEstatus.DesdeTexto(Estatus);
}
