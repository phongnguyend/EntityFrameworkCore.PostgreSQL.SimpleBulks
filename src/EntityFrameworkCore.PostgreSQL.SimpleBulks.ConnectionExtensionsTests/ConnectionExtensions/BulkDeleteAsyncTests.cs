using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkDeleteAsyncTests : BaseTest
{
    private string _schema = "";

    public BulkDeleteAsyncTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkDeleteTest")
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < 100; i++)
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

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3 });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var connectionContext = new ConnectionContext(_connection, null);

        var options = new BulkDeleteOptions
        {
            LogTo = _output.WriteLine
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, options: options);
            }
            else
            {
                await connectionContext.BulkDeleteAsync(rows,
                    new NpgsqlTableInfor<SingleKeyRow<int>>(_schema, "SingleKeyRows")
                    {
                        PrimaryKeys = ["Id"],
                    }, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows,
                    new NpgsqlTableInfor<CompositeKeyRow<int, int>>(_schema, "CompositeKeyRows")
                    {
                        PrimaryKeys = ["Id1", "Id2"],
                    }, options: options);
            }
        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, options: options);
            }
            else
            {
                connectionContext.BulkDelete(rows,
                    new NpgsqlTableInfor<SingleKeyRow<int>>(_schema, "SingleKeyRows")
                    {
                        PrimaryKeys = ["Id"],
                    }, options: options);
                connectionContext.BulkDelete(compositeKeyRows,
                    new NpgsqlTableInfor<CompositeKeyRow<int, int>>(_schema, "CompositeKeyRows")
                    {
                        PrimaryKeys = ["Id1", "Id2"],
                    }, options: options);
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }
}