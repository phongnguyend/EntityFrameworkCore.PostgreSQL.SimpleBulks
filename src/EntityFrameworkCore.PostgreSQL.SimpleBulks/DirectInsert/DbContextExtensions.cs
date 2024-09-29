﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectInsert
{
    public static class DbContextExtensions
    {
        public static void DirectInsert<T>(this DbContext dbContext, T data, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var columns = properties
                .Where(x => x.ValueGenerated == ValueGenerated.Never)
                .Select(x => x.PropertyName);
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithColumns(columns)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOutputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .SingleInsert(data);
        }

        public static void DirectInsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithColumns(columnNamesSelector)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOutputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .SingleInsert(data);
        }
    }
}
