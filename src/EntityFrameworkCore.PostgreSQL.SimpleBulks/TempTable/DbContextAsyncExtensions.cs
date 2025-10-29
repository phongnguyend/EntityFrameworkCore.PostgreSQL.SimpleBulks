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
    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<TempTableOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();

        return new TempTableBuilder<T>(connection, transaction)
             .WithColumns(columnNamesSelector)
             .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .ConfigureTempTableOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<TempTableOptions> configureOptions = null, CancellationToken cancellationToken = default)
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
             .ExecuteAsync(data, cancellationToken);
    }
}
