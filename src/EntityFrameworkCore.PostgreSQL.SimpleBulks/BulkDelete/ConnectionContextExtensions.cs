using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System.Collections.Generic;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, NpgsqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .Execute(data);
    }
}