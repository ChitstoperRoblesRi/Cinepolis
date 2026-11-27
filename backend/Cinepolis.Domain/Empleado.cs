namespace Cinepolis.Domain;

/// <summary>Entidad de empleado del sistema de cine.</summary>
public sealed class Empleado
{
    public int EmpleadoId { get; init; }

    public required string Nombre { get; init; }

    public required string ApellidoPaterno { get; init; }

    public string? ApellidoMaterno { get; init; }

    public string? Telefono { get; init; }

    public decimal Salario { get; init; }

    public DateOnly FechaNacimiento { get; init; }

    public required string Estatus { get; init; }

    public DateOnly FechaContratacion { get; init; }

    public string? Correo { get; init; }

    public int ComplejoId { get; init; }

    public int RolId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public EmpleadoEstatus EstatusTipado => ValoresEmpleadoEstatus.DesdeTexto(Estatus);
}
