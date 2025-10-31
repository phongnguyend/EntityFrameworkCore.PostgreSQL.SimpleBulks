﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("PostgreSqlCollection")]
public class DirectInsertTests : BaseTest
{
    public DirectInsertTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "DirectInsertTest")
    {
    }

    [Fact]
    public void Direct_Insert_Using_Linq_Without_Transaction()
    {
        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer
        };

        _context.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });

        _context.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3.TruncateToMicroseconds(), dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void Direct_Insert_Using_Linq_With_Transaction_Committed()
    {
        var tran = _context.Database.BeginTransaction();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Winter
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Winter
        };

        _context.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });

        _context.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3.TruncateToMicroseconds(), dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void Direct_Insert_Using_Linq_With_Transaction_RolledBack()
    {
        var tran = _context.Database.BeginTransaction();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Summer,
            SeasonAsString = Season.Spring
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Summer,
            SeasonAsString = Season.Spring
        };

        _context.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });

        _context.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions
                {
                    LogTo = _output.WriteLine
                });

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Fact]
    public void Direct_Insert_KeepIdentity()
    {
        var configurationEntry = new ConfigurationEntry
        {
            Id = Guid.NewGuid(),
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        _context.DirectInsert(configurationEntry, new BulkInsertOptions
        {
            KeepIdentity = true,
            LogTo = _output.WriteLine
        });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[0].CreatedDateTime);
    }

    [Fact]
    public void Direct_Insert_Return_DbGeneratedId()
    {
        var configurationEntry = new ConfigurationEntry
        {
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        _context.DirectInsert(configurationEntry, new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.NotEqual(Guid.Empty, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[0].CreatedDateTime);
    }
}