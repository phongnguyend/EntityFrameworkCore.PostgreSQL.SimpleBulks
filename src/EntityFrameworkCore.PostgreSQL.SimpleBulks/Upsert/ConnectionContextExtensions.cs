using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Upsert;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return connectionContext.CreateBulkMergeBuilder<T>()
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, BulkMergeOptions options = null)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return connectionContext.CreateBulkMergeBuilder<T>()
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleMerge(data);
    }
}