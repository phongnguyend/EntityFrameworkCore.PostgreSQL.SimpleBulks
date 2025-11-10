using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(table.PrimaryKeys)
             .ToTable(table)
             .WithBulkOptions(options)
             .ExecuteAsync(data, cancellationToken);
    }
}
