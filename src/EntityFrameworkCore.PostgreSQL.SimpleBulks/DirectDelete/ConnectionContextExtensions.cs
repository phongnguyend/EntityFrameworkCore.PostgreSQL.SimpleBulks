using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, NpgsqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
        .WithId(temp.PrimaryKeys)
           .ToTable(temp)
              .WithBulkOptions(options)
     .SingleDelete(data);
    }
}