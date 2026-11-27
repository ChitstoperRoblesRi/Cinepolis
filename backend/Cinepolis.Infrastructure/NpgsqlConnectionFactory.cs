using Cinepolis.Application;
using System.Data.Common;
using Dapper;
using Npgsql;
using System.Data;

namespace Cinepolis.Infrastructure;

/// <summary>Configura el mapeo de Dapper y abre conexiones PostgreSQL desde el pool.</summary>
public sealed class NpgsqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource source;

    /// <summary>Configura el mapeo de Dapper y abre conexiones PostgreSQL desde el pool.</summary>
    public NpgsqlConnectionFactory(string connectionString)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateHandler());
        SqlMapper.AddTypeHandler(new TimeHandler());
        source = NpgsqlDataSource.Create(connectionString);
    }

    // Npgsql lee timestamptz como DateTime UTC y timestamp como DateTime Unspecified.
    /// <summary>Abre una conexión para la operación solicitada.</summary>
    public async ValueTask<DbConnection> OpenAsync(CancellationToken cancellationToken) =>
        await source.OpenConnectionAsync(cancellationToken);

    /// <summary>Libera el pool de conexiones.</summary>
    public ValueTask DisposeAsync() => source.DisposeAsync();

    private sealed class DateHandler : SqlMapper.TypeHandler<DateOnly>
    {
        /// <summary>Convierte el valor devuelto por PostgreSQL al tipo de dominio.</summary>
        public override DateOnly Parse(object value) => value is DateTime date ? DateOnly.FromDateTime(date) : (DateOnly)value;
        /// <summary>Asigna al parámetro el tipo y el valor temporal correspondientes.</summary>
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value;
        }
    }

    private sealed class TimeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        /// <summary>Convierte el valor devuelto por PostgreSQL al tipo de dominio.</summary>
        public override TimeOnly Parse(object value) => value is TimeSpan time ? TimeOnly.FromTimeSpan(time) : (TimeOnly)value;
        /// <summary>Asigna al parámetro el tipo y el valor temporal correspondientes.</summary>
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value;
        }
    }
}
