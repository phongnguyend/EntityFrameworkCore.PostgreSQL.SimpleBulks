using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkUpdateTests : BaseTest
{
    public BulkUpdateTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkUpdateTest")
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Winter,
                SeasonAsString = Season.Winter
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Winter,
                SeasonAsString = Season.Winter
            });
        }

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void BulkUpdate_PrimaryKeys(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

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

        var updateOptions = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                connectionContext.BulkUpdate(rows,
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows,
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);
            }
            else
            {
                connectionContext.BulkUpdate(rows,
                    row => new { row.Column3, row.Column2 },
                    _singleKeyRowTableInfo,
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows,
                    row => new { row.Column3, row.Column2 },
                    _compositeKeyRowTableInfo,
                    options: updateOptions);
            }
        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkUpdate(rows,
                    ["Column3", "Column2"],
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows,
                    ["Column3", "Column2"],
                    options: updateOptions);
            }
            else
            {
                connectionContext.BulkUpdate(rows,
                    ["Column3", "Column2"],
                    _singleKeyRowTableInfo,
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows,
                    ["Column3", "Column2"],
                    _compositeKeyRowTableInfo,
                    options: updateOptions);
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

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void BulkUpdate_SpecifiedKeys(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

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

        var updateOptions = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                connectionContext.BulkUpdate(rows, x => x.Id,
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows, x => new { x.Id1, x.Id2 },
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);
            }
            else
            {
                connectionContext.BulkUpdate(rows, x => x.Id,
                    row => new { row.Column3, row.Column2 },
                    _singleKeyRowTableInfo,
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows, x => new { x.Id1, x.Id2 },
                    row => new { row.Column3, row.Column2 },
                    _compositeKeyRowTableInfo,
                    options: updateOptions);
            }
        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkUpdate(rows, ["Id"],
                    ["Column3", "Column2"],
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows, ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    options: updateOptions);
            }
            else
            {
                connectionContext.BulkUpdate(rows, ["Id"],
                    ["Column3", "Column2"],
                    _singleKeyRowTableInfo,
                    options: updateOptions);

                connectionContext.BulkUpdate(compositeKeyRows, ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    _compositeKeyRowTableInfo,
                    options: updateOptions);
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