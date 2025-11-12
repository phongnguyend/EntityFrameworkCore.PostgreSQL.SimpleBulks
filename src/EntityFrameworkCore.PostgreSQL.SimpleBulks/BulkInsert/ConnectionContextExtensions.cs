using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public static class ConnectionContextExtensions
{
    public static void BulkInsert<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(temp.InsertablePropertyNames)
       .ToTable(temp)
          .WithBulkOptions(options)
     .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNamesSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
     .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
         .WithColumns(columnNames)
         .ToTable(table ?? TableMapper.Resolve<T>())
              .WithBulkOptions(options)
         .Execute(data);
    }
}