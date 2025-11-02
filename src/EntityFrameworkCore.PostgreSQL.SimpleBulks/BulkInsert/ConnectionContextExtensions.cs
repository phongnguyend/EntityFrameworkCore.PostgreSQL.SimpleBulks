using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public static class ConnectionContextExtensions
{
    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNamesSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
     .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, NpgsqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
      .WithColumns(columnNamesSelector)
      .ToTable(table ?? TableMapper.Resolve<T>())
         .WithOutputId(idSelector)
  .WithBulkOptions(options)
     .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, NpgsqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
         .WithColumns(columnNames)
         .ToTable(table ?? TableMapper.Resolve<T>())
              .WithBulkOptions(options)
         .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, NpgsqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNames)
      .ToTable(table ?? TableMapper.Resolve<T>())
    .WithOutputId(idColumnName)
   .WithBulkOptions(options)
      .Execute(data);
    }
}