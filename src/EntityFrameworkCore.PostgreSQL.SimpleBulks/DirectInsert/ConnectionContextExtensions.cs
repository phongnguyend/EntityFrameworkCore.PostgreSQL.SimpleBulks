using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectInsert;

public static class ConnectionContextExtensions
{
    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNamesSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
     .SingleInsert(data);
    }

    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
         .WithColumns(columnNames)
         .ToTable(table ?? TableMapper.Resolve<T>())
              .WithBulkOptions(options)
         .SingleInsert(data);
    }
}