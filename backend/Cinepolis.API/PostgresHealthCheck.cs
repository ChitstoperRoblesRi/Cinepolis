using Cinepolis.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Cinepolis.Application;

namespace Cinepolis.API;

/// <summary>Verifica la conectividad con PostgreSQL.</summary>
public sealed class PostgresHealthCheck(IDbConnectionFactory factory) : IHealthCheck
{
    /// <summary>Verifica la conectividad con PostgreSQL.</summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        await using var connection = await factory.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        await command.ExecuteScalarAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
