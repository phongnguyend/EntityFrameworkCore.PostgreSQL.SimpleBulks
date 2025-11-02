using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNamesSelector)
             .ToTable(table ?? TableMapper.Resolve<T>())
           .WithOutputId(idSelector)
               .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
  .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
      .WithColumns(columnNames)
     .ToTable(table ?? TableMapper.Resolve<T>())
       .WithOutputId(idColumnName)
        .WithBulkOptions(options)
         .ExecuteAsync(data, cancellationToken);
    }
}