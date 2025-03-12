using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Npgsql;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.NpgsqlConnectionExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;

    protected readonly TestDbContext _context;
    protected readonly NpgsqlConnection _connection;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _output = output;
        var connectionString = GetConnectionString(dbPrefixName);
        _context = GetDbContext(connectionString);
        _context.Database.EnsureCreated();
        _connection = new NpgsqlConnection(connectionString);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"Host=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};Username=postgres;Password=postgres";
    }

    protected TestDbContext GetDbContext(string connectionString)
    {
        return new TestDbContext(connectionString);
    }
}
