﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.DbContextExtensions;

[Collection("PostgreSqlCollection")]
public class BulkInsertTests : BaseTest
{
    public BulkInsertTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkInsertTest")
    {
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Insert_Using_Linq_Without_Transaction(int length)
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn
            });
        }

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3, row.Season },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, compositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Insert_Using_Linq_With_Transaction_Committed(int length)
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
                Season = Season.Autumn
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn
            });
        }

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3, row.Season },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < length; i++)
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
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Insert_Using_Linq_With_Transaction_RolledBack(int length)
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
                row => new { row.Column1, row.Column2, row.Column3, row.Season });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season });

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Insert_KeepIdentity(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries, options =>
        {
            options.KeepIdentity = true;
            options.LogTo = _output.WriteLine;
        });

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Insert_Return_DbGeneratedId(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries, options =>
        {
            options.LogTo = _output.WriteLine;
        });

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.NotEqual(Guid.Empty, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
        }
    }
}