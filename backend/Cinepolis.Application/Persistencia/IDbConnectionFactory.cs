using System.Data.Common;

namespace Cinepolis.Application;

/// <summary>Abre conexiones independientes cuyo ciclo de vida controla el consumidor.</summary>
public interface IDbConnectionFactory
{
    /// <summary>Obtiene una conexión abierta con soporte para transacciones asíncronas.</summary>
    ValueTask<DbConnection> OpenAsync(CancellationToken token);
}
