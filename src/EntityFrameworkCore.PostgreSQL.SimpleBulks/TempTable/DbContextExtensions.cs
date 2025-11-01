using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public static class DbContextExtensions
{
    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null)
    {
        return dbContext.CreateTempTableBuilder<T>()
      .WithColumns(columnNamesSelector)
        .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
 .WithTempTableOptions(options)
    .Execute(data);
    }

    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null)
    {
        var isEntityType = dbContext.IsEntityType(typeof(T));

        if (isEntityType)
        {
            columnNames = dbContext.GetAllPropertyNamesWithoutRowVersions(typeof(T));
        }

        return dbContext.CreateTempTableBuilder<T>()
      .WithColumns(columnNames)
   .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
         .WithTempTableOptions(options)
     .Execute(data);
    }
}
