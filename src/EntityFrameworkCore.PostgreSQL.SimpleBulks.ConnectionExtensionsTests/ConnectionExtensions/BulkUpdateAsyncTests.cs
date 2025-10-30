using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkUpdateAsyncTests : BaseTest
{
    private string _schema = "";

    public BulkUpdateAsyncTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkUpdateTest")
    {
        TableMapper.Register(typeof(SingleKeyRow<int>), _schema, "SingleKeyRows");
        TableMapper.Register(typeof(CompositeKeyRow<int, int>), _schema, "CompositeKeyRows");

        var tran = _context.Database.BeginTransaction();

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

        tran.Commit();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Bulk_Update_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var connectionContext = new ConnectionContext(_connection, null);

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkUpdateAsync(rows,
                     row => row.Id,
                     row => new { row.Column3, row.Column2 },
                     options =>
                     {
                         options.LogTo = _output.WriteLine;
                     });

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                     row => new { row.Id1, row.Id2 },
                     row => new { row.Column3, row.Column2 },
                     options =>
                     {
                         options.LogTo = _output.WriteLine;
                     });
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                      row => row.Id,
                      row => new { row.Column3, row.Column2 },
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });

                await connectionContext.BulkUpdateAsync(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                       row => new { row.Id1, row.Id2 },
                       row => new { row.Column3, row.Column2 },
                       options =>
                       {
                           options.LogTo = _output.WriteLine;
                       });
            }

            var newId = rows.Max(x => x.Id) + 1;

            rows.Add(new SingleKeyRow<int>
            {
                Id = newId,
                Column1 = newId,
                Column2 = "Inserted using Merge" + newId,
                Column3 = DateTime.Now,
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = newId1,
                Id2 = newId2,
                Column1 = newId2,
                Column2 = "Inserted using Merge" + newId2,
                Column3 = DateTime.Now,
            });

            if (omitTableName)
            {
                await connectionContext.BulkMergeAsync(rows,
                      row => row.Id,
                      row => new { row.Column1, row.Column2 },
                      row => new { row.Column1, row.Column2, row.Column3 },
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });

                await connectionContext.BulkMergeAsync(compositeKeyRows,
                     row => new { row.Id1, row.Id2 },
                     row => new { row.Column1, row.Column2, row.Column3 },
                     row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                     options =>
                     {
                         options.LogTo = _output.WriteLine;
                     });
            }
            else
            {
                await connectionContext.BulkMergeAsync(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                     row => row.Id,
                     row => new { row.Column1, row.Column2 },
                     row => new { row.Column1, row.Column2, row.Column3 },
                     options =>
                     {
                         options.LogTo = _output.WriteLine;
                     });

                await connectionContext.BulkMergeAsync(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                     row => new { row.Id1, row.Id2 },
                     row => new { row.Column1, row.Column2, row.Column3 },
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
                await connectionContext.BulkUpdateAsync(rows,
                     ["Id"],
                     ["Column3", "Column2"],
                     options =>
                     {
                         options.LogTo = _output.WriteLine;
                     });

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                      ["Id1", "Id2"],
                      ["Column3", "Column2"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                      ["Id"],
                      ["Column3", "Column2"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });

                await connectionContext.BulkUpdateAsync(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                      ["Id1", "Id2"],
                      ["Column3", "Column2"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });
            }

            var newId = rows.Max(x => x.Id) + 1;

            rows.Add(new SingleKeyRow<int>
            {
                Id = newId,
                Column1 = newId,
                Column2 = "Inserted using Merge" + newId,
                Column3 = DateTime.Now,
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = newId1,
                Id2 = newId2,
                Column1 = newId2,
                Column2 = "Inserted using Merge" + newId2,
                Column3 = DateTime.Now,
            });

            if (omitTableName)
            {
                await connectionContext.BulkMergeAsync(rows,
                      ["Id"],
                      ["Column1", "Column2"],
                      ["Column1", "Column2", "Column3"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });

                await connectionContext.BulkMergeAsync(compositeKeyRows,
                      ["Id1", "Id2"],
                      ["Column1", "Column2", "Column3"],
                      ["Id1", "Id2", "Column1", "Column2", "Column3"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });
            }
            else
            {
                await connectionContext.BulkMergeAsync(rows, new NpgsqlTableInfor(_schema, "SingleKeyRows"),
                      ["Id"],
                      ["Column1", "Column2"],
                      ["Column1", "Column2", "Column3"],
                      options =>
                      {
                          options.LogTo = _output.WriteLine;
                      });

                await connectionContext.BulkMergeAsync(compositeKeyRows, new NpgsqlTableInfor(_schema, "CompositeKeyRows"),
                     ["Id1", "Id2"],
                     ["Column1", "Column2", "Column3"],
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

        for (var i = 0; i < 101; i++)
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