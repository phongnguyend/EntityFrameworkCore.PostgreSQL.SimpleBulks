using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;

    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _output = output;
        _context = GetDbContext(dbPrefixName);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"Host=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};Username=postgres;Password=postgres";
    }

    protected TestDbContext GetDbContext(string dbPrefixName)
    {
        return new TestDbContext(GetConnectionString(dbPrefixName));
    }
}
