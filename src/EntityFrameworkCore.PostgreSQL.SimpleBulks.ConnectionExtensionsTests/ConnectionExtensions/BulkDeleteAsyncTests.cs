using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("PostgreSqlCollection")]
public class BulkDeleteAsyncTests : BaseTest
{
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
                Column3 = DateTime.Now,
                Season = Season.Winter,
                SeasonAsString = Season.Winter,
                ComplexShippingAddress = new ComplexTypeAddress
                {
                    Street = "Street " + i,
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                OwnedShippingAddress = new OwnedTypeAddress
                {
                    Street = "Street " + i,
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                }
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
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BulkDelete_PrimaryKeys(bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        if (omitTableName)
        {
            await connectionContext.BulkDeleteAsync(rows, options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows, options: options);
        }
        else
        {
            await connectionContext.BulkDeleteAsync(rows,
                _singleKeyRowTableInfo,
                options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows,
                _compositeKeyRowTableInfo,
                options: options);
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BulkDelete_SpecifiedKeys(bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        if (omitTableName)
        {
            await connectionContext.BulkDeleteAsync(rows, x => x.Id, options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows, x => new { x.Id1, x.Id2 }, options: options);
        }
        else
        {
            await connectionContext.BulkDeleteAsync(rows, x => x.Id,
                _singleKeyRowTableInfo,
                options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows, x => new { x.Id1, x.Id2 },
                _compositeKeyRowTableInfo,
                options: options);
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BulkDelete_SpecifiedKeys_DynamicString(bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        if (omitTableName)
        {
            await connectionContext.BulkDeleteAsync(rows, ["Id"], options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows, ["Id1", "Id2"], options: options);
        }
        else
        {
            await connectionContext.BulkDeleteAsync(rows, ["Id"],
                _singleKeyRowTableInfo,
                options: options);
            await connectionContext.BulkDeleteAsync(compositeKeyRows, ["Id1", "Id2"],
                _compositeKeyRowTableInfo,
                options: options);
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }
}