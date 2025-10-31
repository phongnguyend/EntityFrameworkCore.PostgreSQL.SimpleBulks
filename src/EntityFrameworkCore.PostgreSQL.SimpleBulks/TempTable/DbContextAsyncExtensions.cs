using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public static class DbContextAsyncExtensions
{
    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new TempTableBuilder<T>(connectionContext)
         .WithColumns(columnNamesSelector)
         .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
          .WithTempTableOptions(options)
         .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();

        var isEntityType = dbContext.IsEntityType(typeof(T));

        if (isEntityType)
        {
            columnNames = dbContext.GetAllPropertyNamesWithoutRowVersions(typeof(T));
        }

        return new TempTableBuilder<T>(connectionContext)
   .WithColumns(columnNames)
 .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
      .WithTempTableOptions(options)
          .ExecuteAsync(data, cancellationToken);
    }
}
