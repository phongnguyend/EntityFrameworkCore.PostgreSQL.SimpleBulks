using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly PostgreSqlFixture _fixture;
    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, PostgreSqlFixture fixture, string dbPrefixName, string schema = "")
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _output = output;
        _fixture = fixture;
        _context = GetDbContext(dbPrefixName, schema);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string dbPrefixName, string schema)
    {
        return new TestDbContext(_fixture.GetConnectionString(dbPrefixName), schema);
    }
}
