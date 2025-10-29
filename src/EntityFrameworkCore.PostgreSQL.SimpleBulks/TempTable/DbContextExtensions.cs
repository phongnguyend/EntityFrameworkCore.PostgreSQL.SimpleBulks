using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public static class DbContextExtensions
{
    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<TempTableOptions> configureOptions = null)
    {
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();

        return new TempTableBuilder<T>(connection, transaction)
             .WithColumns(columnNamesSelector)
             .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .ConfigureTempTableOptions(configureOptions)
             .Execute(data);
    }

    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<TempTableOptions> configureOptions = null)
    {
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();

        var isEntityType = dbContext.IsEntityType(typeof(T));

        if (isEntityType)
        {
            columnNames = dbContext.GetAllPropertyNamesWithoutRowVersions(typeof(T));
        }

        return new TempTableBuilder<T>(connection, transaction)
             .WithColumns(columnNames)
             .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .ConfigureTempTableOptions(configureOptions)
             .Execute(data);
    }
}
