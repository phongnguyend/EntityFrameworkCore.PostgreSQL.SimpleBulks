using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Upsert;

public static class DbContextExtensions
{
    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return dbContext.CreateBulkMergeBuilder<T>()
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .ToTable(dbContext.GetTableInfor<T>())
            .WithBulkOptions(options)
            .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, BulkMergeOptions options = null)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return dbContext.CreateBulkMergeBuilder<T>()
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(dbContext.GetTableInfor<T>())
            .WithBulkOptions(options)
            .SingleMerge(data);
    }
}
