using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert
{
    public static class NpgsqlConnectionExtensions
    {
        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOutputId(idSelector)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOutputId(idColumnName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOutputId(idSelector)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOutputId(idColumnName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
