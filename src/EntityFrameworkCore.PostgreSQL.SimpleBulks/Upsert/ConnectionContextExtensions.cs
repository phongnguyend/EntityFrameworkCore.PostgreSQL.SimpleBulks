using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Upsert;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, NpgsqlTableInfor table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
    .WithId(idSelector)
   .WithUpdateColumns(updateColumnNamesSelector)
       .WithInsertColumns(insertColumnNamesSelector)
    .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
       .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, NpgsqlTableInfor table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
       .WithId(idColumns)
         .WithUpdateColumns(updateColumnNames)
          .WithInsertColumns(insertColumnNames)
       .ToTable(table ?? TableMapper.Resolve<T>())
      .WithBulkOptions(options)
     .SingleMerge(data);
    }
}