using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public class TempTableBuilder<T>
{
    private IEnumerable<string> _columnNames;
    private MappingContext _mappingContext;
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

    public TempTableBuilder<T> WithMappingContext(MappingContext mappingContext)
    {
        _mappingContext = mappingContext;
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

    public string Execute(IEnumerable<T> data)
    {
        var tempTableName = $"\"{GetTableName()}\"";
        var sqlCreateTempTable = typeof(T).GenerateTempTableDefinition(tempTableName, _columnNames, _mappingContext.ColumnNameMappings, _mappingContext.ColumnTypeMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        _connection.EnsureOpen();
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            createTempTableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        data.SqlBulkCopy(tempTableName, _columnNames, _mappingContext.ColumnNameMappings, false, _connection, _transaction, valueConverters: _mappingContext.ValueConverters);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [TempTable]: {message}");
    }

    public async Task<string> ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        var tempTableName = $"\"{GetTableName()}\"";
        var sqlCreateTempTable = typeof(T).GenerateTempTableDefinition(tempTableName, _columnNames, _mappingContext.ColumnNameMappings, _mappingContext.ColumnTypeMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        await _connection.EnsureOpenAsync(cancellationToken);
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        await data.SqlBulkCopyAsync(tempTableName, _columnNames, _mappingContext.ColumnNameMappings, false, _connection, _transaction, valueConverters: _mappingContext.ValueConverters, cancellationToken: cancellationToken);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }
}
