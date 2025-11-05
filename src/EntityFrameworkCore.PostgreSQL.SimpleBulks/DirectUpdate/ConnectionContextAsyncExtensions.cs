using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
    .WithId(idSelector)
.WithColumns(columnNamesSelector)
    .ToTable(table ?? TableMapper.Resolve<T>())
 .WithBulkOptions(options)
   .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, NpgsqlTableInfor table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
     .WithId(idColumns)
    .WithColumns(columnNames)
 .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
        .SingleUpdateAsync(data, cancellationToken);
    }
}