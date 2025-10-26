using Npgsql;
using Testcontainers.PostgreSql;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests;

public class PostgreSqlFixture : IAsyncLifetime
{
    private bool UseContainer => true;

    public PostgreSqlContainer? Container { get; }

    public PostgreSqlFixture()
    {
        if (!UseContainer)
        {
            return;
        }

        Container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.DisposeAsync();
    }

    public string GetConnectionString(string dbPrefixName)
    {
        if (!UseContainer)
        {
            return $"Host=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};Username=postgres;Password=postgres";
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(Container!.GetConnectionString());
        connectionStringBuilder.Database = $"{dbPrefixName}.{Guid.NewGuid()}";

        return connectionStringBuilder.ToString();
    }
}

