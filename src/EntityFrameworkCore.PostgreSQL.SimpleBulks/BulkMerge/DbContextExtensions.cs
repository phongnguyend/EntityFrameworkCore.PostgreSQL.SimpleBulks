﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge
{
    public static class DbContextExtensions
    {
        public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
            var outputIdColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();

            return new BulkMergeBuilder<T>(connection, transaction)
                 .WithData(data)
                 .WithId(idSelector)
                 .WithUpdateColumns(updateColumnNamesSelector)
                 .WithInsertColumns(insertColumnNamesSelector)
                 .WithDbColumnMappings(dbColumnMappings)
                 .WithOutputId(outputIdColumn)
                 .ToTable(tableName)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute();
        }

        public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
            var outputIdColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();

            return new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .WithDbColumnMappings(dbColumnMappings)
                .WithOutputId(outputIdColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
            var outputIdColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();

            return new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .WithDbColumnMappings(dbColumnMappings)
                .WithOutputId(outputIdColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
