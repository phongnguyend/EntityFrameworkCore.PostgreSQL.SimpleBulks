using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkDeleteTests : BaseTest
{
    public BulkDeleteTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkDeleteTest")
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

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var connectionContext = new ConnectionContext(_connection, null);

        var options = new BulkDeleteOptions
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                connectionContext.BulkDelete(rows, options: options);
                connectionContext.BulkDelete(compositeKeyRows, options: options);
            }
            else
            {
                connectionContext.BulkDelete(rows,
                    new NpgsqlTableInfor<SingleKeyRow<int>>(GetSchema(), "SingleKeyRows")
                    {
                        PrimaryKeys = ["Id"],
                    }, options: options);
                connectionContext.BulkDelete(compositeKeyRows,
                    new NpgsqlTableInfor<CompositeKeyRow<int, int>>(GetSchema(), "CompositeKeyRows")
                    {
                        PrimaryKeys = ["Id1", "Id2"],
                    }, options: options);
            }
        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkDelete(rows, options: options);
                connectionContext.BulkDelete(compositeKeyRows, options: options);
            }
            else
            {
                connectionContext.BulkDelete(rows,
                    new NpgsqlTableInfor<SingleKeyRow<int>>(GetSchema(), "SingleKeyRows")
                    {
                        PrimaryKeys = ["Id"],
                    }, options: options);
                connectionContext.BulkDelete(compositeKeyRows,
                    new NpgsqlTableInfor<CompositeKeyRow<int, int>>(GetSchema(), "CompositeKeyRows")
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