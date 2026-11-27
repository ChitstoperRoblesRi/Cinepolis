using Cinepolis.Application;

namespace Cinepolis.Infrastructure;

/// <summary>Libera la conexión y revierte automáticamente cualquier transacción inconclusa.</summary>
public sealed class DapperUnitOfWork(IDbConnectionFactory factory, DulceriaRepository dulceria) : IUnitOfWork
{
    /// <inheritdoc/>
    public async Task<T> EjecutarAsync<T>(Func<ITransaccionOrden, Task<T>> operacion, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        var resultado = await operacion(new TransaccionOrden(db, tx, dulceria, token));
        await tx.CommitAsync(token);
        return resultado;
    }
}
