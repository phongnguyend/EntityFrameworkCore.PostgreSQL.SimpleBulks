﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public static class NpgsqlConnectionExtensions
{
    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithOutputId(idSelector)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithOutputId(idColumnName)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
    {
        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
    {
        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithOutputId(idSelector)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
    {
        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static void BulkInsert<T>(this NpgsqlConnection connection, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
    {
        new BulkInsertBuilder<T>(connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithOutputId(idColumnName)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }
}
