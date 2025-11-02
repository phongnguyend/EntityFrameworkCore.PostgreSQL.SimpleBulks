using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, NpgsqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
  .WithId(idSelector)
   .WithColumns(columnNamesSelector)
      .ToTable(table ?? TableMapper.Resolve<T>())
  .WithBulkOptions(options)
  .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, NpgsqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
       .WithId(idColumns)
            .WithColumns(columnNames)
       .ToTable(table ?? TableMapper.Resolve<T>())
           .WithBulkOptions(options)
          .Execute(data);
    }
}