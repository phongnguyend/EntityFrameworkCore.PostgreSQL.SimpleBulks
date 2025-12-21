using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .WithColumns(columnNamesSelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, NpgsqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .WithColumns(columnNames)
            .ToTable(temp)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> keySelector, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keySelector)
            .WithColumns(columnNamesSelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> keys, IReadOnlyCollection<string> columnNames, NpgsqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keys)
            .WithColumns(columnNames)
            .ToTable(temp)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }
}