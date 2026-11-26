using Cinepolis.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Cinepolis.API;

public sealed class PostgresHealthCheck(NpgsqlConnectionFactory factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        await using var connection = await factory.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
