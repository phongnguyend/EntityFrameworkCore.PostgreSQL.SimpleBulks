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
    protected readonly NpgsqlTableInfor<SingleKeyRow<int>> _singleKeyRowTableInfo;
    protected readonly NpgsqlTableInfor<CompositeKeyRow<int, int>> _compositeKeyRowTableInfo;

    protected BaseTest(ITestOutputHelper output, PostgreSqlFixture fixture, string dbPrefixName)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);
        string schema = GetSchema();
        _context = GetDbContext(connectionString, schema);
        _context.Database.EnsureCreated();
        _connection = new NpgsqlConnection(connectionString);

        _singleKeyRowTableInfo = new NpgsqlTableInfor<SingleKeyRow<int>>(schema, "SingleKeyRows")
        {
            PrimaryKeys = ["Id"],
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            },
            ColumnTypeMappings = new Dictionary<string, string>
            {
                {"SeasonAsString", "text" }
            },
            ValueConverters = new Dictionary<string, ValueConverter>
            {
                {"SeasonAsString", new ValueConverter(typeof(string),x => x.ToString(),v => (Season)Enum.Parse(typeof(Season), (string)v))}
            }
        };

        _compositeKeyRowTableInfo = new NpgsqlTableInfor<CompositeKeyRow<int, int>>(schema, "CompositeKeyRows")
        {
            PrimaryKeys = ["Id1", "Id2"],
            ColumnTypeMappings = new Dictionary<string, string>
            {
                {"SeasonAsString", "text" }
            },
            ValueConverters = new Dictionary<string, ValueConverter>
            {
                {"SeasonAsString", new ValueConverter(typeof(string),x => x.ToString(),v => (Season)Enum.Parse(typeof(Season), (string)v))}
            }
        };

        TableMapper.Configure<SingleKeyRow<int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("SingleKeyRows")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "text")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
        });

        TableMapper.Configure<CompositeKeyRow<int, int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("CompositeKeyRows")
            .PrimaryKeys(x => new { x.Id1, x.Id2 })
            .ConfigureProperty(x => x.SeasonAsString, columnType: "text")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
        });

        TableMapper.Configure<ConfigurationEntry>(config =>
        {
            config
            .Schema(schema)
            .TableName("ConfigurationEntry")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated)
            .ReadOnlyProperty(x => x.RowVersion);
        });

        TableMapper.Configure<Customer>(config =>
        {
            config
            .Schema(schema)
            .TableName("Customers")
            .IgnoreProperty(x => x.Contacts)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "text")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
        });

        TableMapper.Configure<Contact>(config =>
        {
            config
            .Schema(schema)
            .TableName("Contacts")
            .IgnoreProperty(x => x.Customer)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "text")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
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

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }

    protected string GetSchema()
    {
        return Environment.GetEnvironmentVariable("SCHEMA") ?? "";
    }
}
