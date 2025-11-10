using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(table.PrimaryKeys)
             .ToTable(table)
             .WithBulkOptions(options)
             .Execute(data);
    }
}
