﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.DbContextExtensions;

[Collection("PostgreSqlCollection")]
public class BulkDeleteTests : BaseTest
{
    public BulkDeleteTests(ITestOutputHelper output, PostgreSqlFixture fixture) : base(output, fixture, "BulkDeleteTest")
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
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Delete_Using_Linq_With_Transaction(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().Take(length).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(length).ToList();

        var deleteResult1 = _context.BulkDelete(rows,
                  options =>
                  {
                      options.LogTo = _output.WriteLine;
                  });

        var deleteResult2 = _context.BulkDelete(compositeKeyRows,
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, deleteResult1.AffectedRows);
        Assert.Equal(length, deleteResult2.AffectedRows);

        Assert.Equal(100 - length, dbRows.Count);
        Assert.Equal(100 - length, dbCompositeKeyRows.Count);
    }
}