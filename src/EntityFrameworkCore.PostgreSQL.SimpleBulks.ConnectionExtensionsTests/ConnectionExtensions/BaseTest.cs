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

        TableMapper.Register<SingleKeyRow<int>>(new NpgsqlTableInfor(schema, "SingleKeyRows")
        {
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });

        TableMapper.Register<CompositeKeyRow<int, int>>(new NpgsqlTableInfor(schema, "CompositeKeyRows"));

        TableMapper.Register<ConfigurationEntry>(new NpgsqlTableInfor(schema, "ConfigurationEntry")
        {
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });

        TableMapper.Register<Customer>(new NpgsqlTableInfor(schema, "Customers")
        {
            PropertyNames = ["Id", "FirstName", "LastName", "CurrentCountryIsoCode", "Index", "Season", "SeasonAsString"]
        });

        TableMapper.Register<Contact>(new NpgsqlTableInfor(schema, "Contacts")
        {
            PropertyNames = ["Id", "EmailAddress", "PhoneNumber", "CountryIsoCode", "Index", "Season", "SeasonAsString", "CustomerId"]
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
