using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, NpgsqlTableInfor table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }
}