using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("PostgreSqlCollection")]
public class DirectUpdateTests : BaseTest
{
    public DirectUpdateTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "DirectUpdateTest")
    {
    }

    private void SeedData(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Summer,
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
                Id1 = i + 1,
                Id2 = i + 1,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Summer
            });
        }

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(90)]
    public void Direct_Update_Using_Linq_With_Transaction(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Winter;
        row.SeasonAsString = Season.Autumn;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Winter;
        compositeKeyRow.SeasonAsString = Season.Autumn;

        var updateResult1 = _context.DirectUpdate(row,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                new BulkUpdateOptions
                {
                    LogTo = _output.WriteLine
                });

        var updateResult2 = _context.DirectUpdate(compositeKeyRow,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                new BulkUpdateOptions
                {
                    LogTo = _output.WriteLine
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.OrderBy(x => x.Id).AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.OrderBy(x => x.Id1).ThenBy(x => x.Id2).AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(90)]
    public void Direct_Update_Using_Dynamic_String_With_Transaction(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Winter;
        row.SeasonAsString = Season.Autumn;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Winter;
        compositeKeyRow.SeasonAsString = Season.Autumn;

        var updateResult1 = _context.DirectUpdate(row,
              ["Column3", "Column2", "Season", "SeasonAsString"],
              new BulkUpdateOptions
              {
                  LogTo = _output.WriteLine
              });

        var updateResult2 = _context.DirectUpdate(compositeKeyRow,
            ["Column3", "Column2", "Season", "SeasonAsString"],
          new BulkUpdateOptions
          {
              LogTo = _output.WriteLine
          });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.OrderBy(x => x.Id).AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.OrderBy(x => x.Id1).ThenBy(x => x.Id2).AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }
}