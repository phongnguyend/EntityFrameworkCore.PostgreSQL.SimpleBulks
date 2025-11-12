using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using Npgsql;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

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

        TableMapper.Configure<SingleKeyRow<int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("SingleKeyRows")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated);
        });

        TableMapper.Configure<CompositeKeyRow<int, int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("CompositeKeyRows")
            .PrimaryKeys(x => new { x.Id1, x.Id2 });
        });

        TableMapper.Configure<ConfigurationEntry>(config =>
        {
            config
            .Schema(schema)
            .TableName("ConfigurationEntry")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated);
        });

        TableMapper.Configure<Customer>(config =>
        {
            config
            .Schema(schema)
            .TableName("Customers")
            .PropertyNames(["Id", "FirstName", "LastName", "CurrentCountryIsoCode", "Index", "Season", "SeasonAsString"]);
        });

        TableMapper.Configure<Contact>(config =>
        {
            config
            .Schema(schema)
            .TableName("Contacts")
            .PropertyNames(["Id", "EmailAddress", "PhoneNumber", "CountryIsoCode", "Index", "Season", "SeasonAsString", "CustomerId"]);
        });
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
