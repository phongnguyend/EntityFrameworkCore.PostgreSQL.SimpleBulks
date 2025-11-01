using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null)
    {
        return new BulkDeleteBuilder<T>(dbContext.GetConnectionContext())
        .WithId(dbContext.GetPrimaryKeys(typeof(T)))
      .ToTable(dbContext.GetTableInfor(typeof(T)))
    .WithBulkOptions(options)
      .SingleDelete(data);
    }
}
