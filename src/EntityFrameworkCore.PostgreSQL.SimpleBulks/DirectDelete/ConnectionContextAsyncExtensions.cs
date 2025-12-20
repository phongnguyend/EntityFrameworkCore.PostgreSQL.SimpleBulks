using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, NpgsqlTableInfor<T> table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDeleteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> keySelector, NpgsqlTableInfor<T> table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDeleteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> keys, NpgsqlTableInfor<T> table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDeleteAsync(data, cancellationToken);
    }
}