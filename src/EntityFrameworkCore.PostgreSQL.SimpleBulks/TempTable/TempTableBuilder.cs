﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public class TempTableBuilder<T>
{
    private IEnumerable<T> _data;
    private IEnumerable<string> _columnNames;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private TempTableOptions _options;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    public TempTableBuilder(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public TempTableBuilder(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public TempTableBuilder<T> WithData(IEnumerable<T> data)
    {
        _data = data;
        return this;
    }

    public TempTableBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public TempTableBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public TempTableBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public TempTableBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public TempTableBuilder<T> ConfigureTempTableOptions(Action<TempTableOptions> configureOptions)
    {
        _options = new TempTableOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetTableName()
    {
        if (!string.IsNullOrWhiteSpace(_options.TableName))
        {
            return _options.TableName;
        }

        if (!string.IsNullOrWhiteSpace(_options.PrefixName))
        {
            return _options.PrefixName + "-" + Guid.NewGuid();
        }

        return Guid.NewGuid().ToString();
    }

    public string Execute()
    {
        var tempTableName = $"\"{GetTableName()}\"";
        var sqlCreateTempTable = typeof(T).GenerateTempTableDefinition(tempTableName, _columnNames, _columnNameMappings, _columnTypeMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        _connection.EnsureOpen();
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            createTempTableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        _data.SqlBulkCopy(tempTableName, _columnNames, _columnNameMappings, false, _connection, _transaction);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [TempTable]: {message}");
    }
}
