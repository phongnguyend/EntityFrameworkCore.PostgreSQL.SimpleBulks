using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkInsertTests : BaseTest
{
    private string _schema = "";

    public BulkInsertTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkInsertTest")
    {
        TableMapper.Register<SingleKeyRow<int>>(new NpgsqlTableInfor(_schema, "SingleKeyRows"));
        TableMapper.Register<CompositeKeyRow<int, int>>(new NpgsqlTableInfor(_schema, "CompositeKeyRows"));
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

        var connectionContext = new ConnectionContext(_connection, null);

        if (useLinq)
        {
            if (omitTableName)
            {
                connectionContext.BulkInsert(rows,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => row.Id,
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });

                connectionContext.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });
            }
            else
            {
                connectionContext.BulkInsert(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => row.Id,
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });

                connectionContext.BulkInsert(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });
            }

        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkInsert(rows,
                    ["Column1", "Column2", "Column3"],
                    "Id",
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });

                connectionContext.BulkInsert(compositeKeyRows,
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });
            }
            else
            {
                connectionContext.BulkInsert(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                    ["Column1", "Column2", "Column3"],
                    "Id",
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
                    });

                connectionContext.BulkInsert(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    new BulkInsertOptions
                    {
                        LogTo = _output.WriteLine
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