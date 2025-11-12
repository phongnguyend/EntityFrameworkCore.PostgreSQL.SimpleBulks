using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, NpgsqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
  .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }
}