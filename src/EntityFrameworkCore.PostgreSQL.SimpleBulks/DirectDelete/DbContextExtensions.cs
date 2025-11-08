using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(table.PrimaryKeys)
             .ToTable(table)
    .WithBulkOptions(options)
      .SingleDelete(data);
    }
}
