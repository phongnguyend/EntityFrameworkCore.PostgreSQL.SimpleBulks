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
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(dbContext.GetConnectionContext())
      .WithId(idSelector)
        .WithUpdateColumns(updateColumnNamesSelector)
     .WithInsertColumns(insertColumnNamesSelector)
          .WithOutputId(outputIdColumn)
       .ToTable(dbContext.GetTableInfor(typeof(T)))
       .WithBulkOptions(options)
          .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(dbContext.GetConnectionContext())
    .WithId(idColumns)
   .WithUpdateColumns(updateColumnNames)
   .WithInsertColumns(insertColumnNames)
      .WithOutputId(outputIdColumn)
     .ToTable(dbContext.GetTableInfor(typeof(T)))
      .WithBulkOptions(options)
   .SingleMerge(data);
    }
}
