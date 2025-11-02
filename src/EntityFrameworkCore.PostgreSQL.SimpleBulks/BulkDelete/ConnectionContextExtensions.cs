using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, NpgsqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
        .WithId(idSelector)
           .ToTable(table ?? TableMapper.Resolve<T>())
              .WithBulkOptions(options)
     .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, NpgsqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
             .WithId(idColumns)
    .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
              .Execute(data);
    }
}