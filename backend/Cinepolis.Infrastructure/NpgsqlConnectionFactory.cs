using Dapper;
using Npgsql;
using System.Data;

namespace Cinepolis.Infrastructure;

public sealed class NpgsqlConnectionFactory : IAsyncDisposable
{
    private readonly NpgsqlDataSource source;

    public NpgsqlConnectionFactory(string connectionString)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateHandler());
        SqlMapper.AddTypeHandler(new TimeHandler());
        source = NpgsqlDataSource.Create(connectionString);
    }

    // Npgsql lee timestamptz como DateTime UTC y timestamp como DateTime Unspecified.
    public ValueTask<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken) =>
        source.OpenConnectionAsync(cancellationToken);

    public ValueTask DisposeAsync() => source.DisposeAsync();

    private sealed class DateHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) => value is DateTime date ? DateOnly.FromDateTime(date) : (DateOnly)value;
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value;
        }
    }

    private sealed class TimeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(object value) => value is TimeSpan time ? TimeOnly.FromTimeSpan(time) : (TimeOnly)value;
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value;
        }
    }
}
