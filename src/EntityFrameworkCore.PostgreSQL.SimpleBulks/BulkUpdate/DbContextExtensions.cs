﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var primaryKeys = properties
            .Where(x => x.IsPrimaryKey)
            .Select(x => x.PropertyName);

        return new BulkUpdateBuilder<T>(connection, transaction)
             .WithId(primaryKeys)
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
             .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetNpgsqlConnection();
        var transaction = dbContext.GetCurrentNpgsqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var primaryKeys = properties
            .Where(x => x.IsPrimaryKey)
            .Select(x => x.PropertyName);

        return new BulkUpdateBuilder<T>(connection, transaction)
            .WithId(primaryKeys)
            .WithColumns(columnNames)
            .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
            .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }
}
