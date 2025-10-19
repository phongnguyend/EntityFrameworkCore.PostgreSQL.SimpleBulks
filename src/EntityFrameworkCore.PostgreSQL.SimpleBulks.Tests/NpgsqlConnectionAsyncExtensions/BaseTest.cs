using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Npgsql;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.NpgsqlConnectionAsyncExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    private readonly PostgreSqlFixture _fixture;
    protected readonly TestDbContext _context;
    protected readonly NpgsqlConnection _connection;

    protected BaseTest(ITestOutputHelper output, PostgreSqlFixture fixture, string dbPrefixName, string schema = "")
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);
        _context = GetDbContext(connectionString, schema);
        _context.Database.EnsureCreated();
        _connection = new NpgsqlConnection(connectionString);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string connectionString, string schema)
    {
        return new TestDbContext(connectionString, schema);
    }
}
