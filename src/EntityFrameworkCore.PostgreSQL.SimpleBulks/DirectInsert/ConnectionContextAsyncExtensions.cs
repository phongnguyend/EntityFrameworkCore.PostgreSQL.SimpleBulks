using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
            .SingleInsertAsync(data, cancellationToken);
    }


    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, NpgsqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
  .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .SingleInsertAsync(data, cancellationToken);
    }
}