using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkDeleteBuilder<T>(dbContext.GetConnectionContext())
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
             .SingleDeleteAsync(data, cancellationToken);
    }
}
