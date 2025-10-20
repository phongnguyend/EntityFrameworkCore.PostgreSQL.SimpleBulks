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

        var isEntityType = dbContext.IsEntityType(typeof(T));

        IReadOnlyDictionary<string, string> columnNameMappings = null;
        IReadOnlyDictionary<string, string> columnTypeMappings = null;

        if (isEntityType)
        {
            columnNameMappings = dbContext.GetColumnNames(typeof(T));
            columnTypeMappings = dbContext.GetColumnTypes(typeof(T));
        }

        return new TempTableBuilder<T>(connection, transaction)
             .WithData(data)
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(columnNameMappings)
             .WithDbColumnTypeMappings(columnTypeMappings)
             .ConfigureTempTableOptions(configureOptions)
             .ExecuteAsync(cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<TempTableOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();

        var isEntityType = dbContext.IsEntityType(typeof(T));

        IReadOnlyDictionary<string, string> columnNameMappings = null;
        IReadOnlyDictionary<string, string> columnTypeMappings = null;

        if (isEntityType)
        {
            columnNames = dbContext.GetAllPropertyNamesWithoutRowVersions(typeof(T));
            columnNameMappings = dbContext.GetColumnNames(typeof(T));
            columnTypeMappings = dbContext.GetColumnTypes(typeof(T));
        }

        return new TempTableBuilder<T>(connection, transaction)
             .WithData(data)
             .WithColumns(columnNames)
             .WithDbColumnMappings(columnNameMappings)
             .WithDbColumnTypeMappings(columnTypeMappings)
             .ConfigureTempTableOptions(configureOptions)
             .ExecuteAsync(cancellationToken);
    }
}
