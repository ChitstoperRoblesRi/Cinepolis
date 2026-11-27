namespace Cinepolis.Application;

/// <summary>Delimita una operación de persistencia que utiliza una sola conexión y transacción.</summary>
public interface IUnitOfWork
{
    /// <summary>Confirma el resultado únicamente si termina toda la operación.</summary>
    Task<T> EjecutarAsync<T>(Func<ITransaccionOrden, Task<T>> operacion, CancellationToken token);
}
