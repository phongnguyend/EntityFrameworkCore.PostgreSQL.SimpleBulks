using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.CustomSchema;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.NpgsqlConnectionExtensions;

public class BulkInsertTests : IDisposable
{

    private TestDbContext _context;
    private NpgsqlConnection _connection;
    private readonly ITestOutputHelper _output;

    public BulkInsertTests(ITestOutputHelper output)
    {
        _output = output;

        var connectionString = $"Host=127.0.0.1;Database=BulkInsertTest.{Guid.NewGuid()};Username=postgres;Password=postgres";
        _context = new TestDbContext(connectionString);
        _context.Database.EnsureCreated();

        _connection = new NpgsqlConnection(connectionString);

        TableMapper.Register(typeof(SingleKeyRow<int>), TestConstants.Schema, "SingleKeyRows");
        TableMapper.Register(typeof(CompositeKeyRow<int, int>), TestConstants.Schema, "CompositeKeyRows");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Bulk_Insert_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now
            });
        }

        if (useLinq)
        {
            if (omitTableName)
            {
                _connection.BulkInsert(rows,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => row.Id,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkInsert(rows, new TableInfor(TestConstants.Schema, "SingleKeyRows"),
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => row.Id,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows, new TableInfor(TestConstants.Schema, "CompositeKeyRows"),
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

        }
        else
        {
            if (omitTableName)
            {
                _connection.BulkInsert(rows,
                    ["Column1", "Column2", "Column3"],
                    "Id",
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows,
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkInsert(rows, new TableInfor(TestConstants.Schema, "SingleKeyRows"),
                    ["Column1", "Column2", "Column3"],
                    "Id",
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows, new TableInfor(TestConstants.Schema, "CompositeKeyRows"),
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

        }


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
        }
    }
}