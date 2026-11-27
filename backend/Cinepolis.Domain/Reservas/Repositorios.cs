namespace Cinepolis.Domain;

/// <summary>Estado confirmado de una butaca para su difusión.</summary>
public sealed record CambioEstadoAsiento(int FuncionId, int AsientoId, string Estado);

/// <summary>Gestiona la titularidad de reservas de butacas.</summary>
public interface IReservaAsientosRepository
{
    /// <summary>Reserva una butaca disponible para la conexión solicitante.</summary>
    Task<bool> SeleccionarAsync(int funcionId, int asientoId, string conexion, CancellationToken token);
    /// <summary>Libera una butaca reservada por la conexión solicitante.</summary>
    Task<bool> LiberarAsync(int funcionId, int asientoId, string conexion, CancellationToken token);
}

/// <summary>Libera reservas que llegaron a su vencimiento.</summary>
public interface IReservasVencidasRepository
{
    /// <summary>Libera las reservas caducadas y devuelve los cambios confirmados.</summary>
    Task<IReadOnlyList<CambioEstadoAsiento>> LiberarVencidosAsync(CancellationToken token);
}
