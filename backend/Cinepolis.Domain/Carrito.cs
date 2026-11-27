namespace Cinepolis.Domain;

/// <summary>Entidad de carrito del sistema de cine.</summary>
public sealed class Carrito
{
    public int CarritoId { get; init; }

    public string? UuidSesionWeb { get; init; }

    public DateTime FechaCreacion { get; init; }

    public DateTime FechaExpiracion { get; init; }

    public required string Estado { get; init; }

    public int? SesionId { get; init; }

    public int? SocioId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public CarritoEstado EstadoTipado => ValoresCarritoEstado.DesdeTexto(Estado);
}
