using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, NpgsqlTableInfor<T> table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
    .WithId(idSelector)
 .WithUpdateColumns(updateColumnNamesSelector)
     .WithInsertColumns(insertColumnNamesSelector)
 .ToTable(table ?? TableMapper.Resolve<T>())
   .WithBulkOptions(options)
       .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, NpgsqlTableInfor<T> table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
        .WithId(idColumns)
       .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
      .ToTable(table ?? TableMapper.Resolve<T>())
           .WithBulkOptions(options)
            .Execute(data);
    }
}