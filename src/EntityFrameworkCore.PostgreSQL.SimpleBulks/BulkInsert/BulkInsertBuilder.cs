using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;

public class BulkInsertBuilder<T>
{
    private TableInfor _table;
    private string _outputIdColumn;
    private OutputIdMode _outputIdMode = OutputIdMode.ServerGenerated;
    private IEnumerable<string> _columnNames;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private IReadOnlyDictionary<string, ValueConverter> _valueConverters;
    private BulkInsertOptions _options;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    public BulkInsertBuilder(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public BulkInsertBuilder(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkInsertBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkInsertBuilder<T> WithOutputId(string idColumn)
    {
        _outputIdColumn = idColumn;
        return this;
    }

    public BulkInsertBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
    {
        _outputIdColumn = idSelector.Body.GetMemberName();
        return this;
    }

    public BulkInsertBuilder<T> WithOutputIdMode(OutputIdMode outputIdMode)
    {
        _outputIdMode = outputIdMode;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkInsertBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public BulkInsertBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public BulkInsertBuilder<T> WithValueConverters(IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
        return this;
    }

    public BulkInsertBuilder<T> ConfigureBulkOptions(Action<BulkInsertOptions> configureOptions)
    {
        _options = new BulkInsertOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetDbColumnName(string columnName)
    {
        if (_columnNameMappings == null)
        {
            return columnName;
        }

        return _columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    private bool ReturnGeneratedId => !string.IsNullOrWhiteSpace(_outputIdColumn);

    private PropertyInfo GetIdProperty()
    {
        return typeof(T).GetProperty(_outputIdColumn);
    }

    private static Action<T, Guid> GetSetIdMethod(PropertyInfo idProperty)
    {
        return (Action<T, Guid>)Delegate.CreateDelegate(typeof(Action<T, Guid>), idProperty.GetSetMethod());
    }

    public void Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            SingleInsert(data.First());
            return;
        }

        if (!ReturnGeneratedId)
        {
            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            data.SqlBulkCopy(_table.SchemaQualifiedTableName, _columnNames, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            data.SqlBulkCopy(_table.SchemaQualifiedTableName, columnsToInsert, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var idProperty = GetIdProperty();

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            data.SqlBulkCopy(_table.SchemaQualifiedTableName, columnsToInsert, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"\"{Guid.NewGuid()}\"";
        var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, _columnNames, null, _columnTypeMappings, addIndexNumberColumn: true);

        var mergeStatementBuilder = new StringBuilder();
        mergeStatementBuilder.AppendLine($"MERGE INTO {_table.SchemaQualifiedTableName} AS a");
        mergeStatementBuilder.AppendLine("USING");
        mergeStatementBuilder.AppendLine("(");
        mergeStatementBuilder.AppendLine($"SELECT * FROM {temptableName} ORDER BY \"{Constants.AutoGeneratedIndexNumberColumn}\"");
        mergeStatementBuilder.AppendLine(") AS b");
        mergeStatementBuilder.AppendLine("ON 1 = 0");
        mergeStatementBuilder.AppendLine("WHEN NOT MATCHED THEN");
        mergeStatementBuilder.AppendLine($"INSERT ({string.Join(", ", _columnNames.Select(x => $"\"{GetDbColumnName(x)}\""))})");
        mergeStatementBuilder.AppendLine($"VALUES ({string.Join(", ", _columnNames.Select(x => $"b.\"{x}\""))})");
        mergeStatementBuilder.AppendLine($"RETURNING a.\"{GetDbColumnName(_outputIdColumn)}\", b.\"{Constants.AutoGeneratedIndexNumberColumn}\"");
        mergeStatementBuilder.AppendLine(";");

        _connection.EnsureOpen();

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");
        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }
        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        data.SqlBulkCopy(temptableName, _columnNames, null, true, _connection, _transaction, _options, valueConverters: _valueConverters);
        Log("End executing SqlBulkCopy.");

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connection.CreateTextCommand(_transaction, sqlMergeStatement, _options))
        {
            using var reader = updateCommand.ExecuteReader();
            var dbColumn = GetDbColumnName(_outputIdColumn);
            while (reader.Read())
            {
                returnedIds[(reader[Constants.AutoGeneratedIndexNumberColumn] as long?).Value] = reader[dbColumn];
            }
        }
        Log("End merging temp table.");

        long idx = 0;
        foreach (var row in data)
        {
            idProperty.SetValue(row, returnedIds[idx]);
            idx++;
        }
    }

    public void SingleInsert(T dataToInsert)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else if (ReturnGeneratedId && _outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);
            setId(dataToInsert, SequentialGuidGenerator.Next());

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

            if (ReturnGeneratedId)
            {
                insertStatementBuilder.AppendLine($"RETURNING \"{GetDbColumnName(_outputIdColumn)}\"");
            }

        }

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);

        _table.CreateSqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        _connection.EnsureOpen();

        if (_options.KeepIdentity || !ReturnGeneratedId)
        {
            var affectedRow = insertCommand.ExecuteNonQuery();
        }
        else
        {
            var dbColumn = GetDbColumnName(_outputIdColumn);
            var idProperty = GetIdProperty();

            using var reader = insertCommand.ExecuteReader();
            while (reader.Read())
            {
                var returnedId = reader[dbColumn];

                idProperty.SetValue(dataToInsert, returnedId);
                break;
            }
        }

        Log($"End inserting.");
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert]: {message}");
    }

    public async Task ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count() == 1)
        {
            await SingleInsertAsync(data.First(), cancellationToken);
            return;
        }

        if (!ReturnGeneratedId)
        {
            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await data.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _columnNames, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters, cancellationToken: cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await data.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, columnsToInsert, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters, cancellationToken: cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var idProperty = GetIdProperty();

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await data.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, columnsToInsert, _columnNameMappings, false, _connection, _transaction, _options, valueConverters: _valueConverters, cancellationToken: cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"\"{Guid.NewGuid()}\"";
        var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, _columnNames, null, _columnTypeMappings, addIndexNumberColumn: true);

        var mergeStatementBuilder = new StringBuilder();
        mergeStatementBuilder.AppendLine($"MERGE INTO {_table.SchemaQualifiedTableName} AS a");
        mergeStatementBuilder.AppendLine("USING");
        mergeStatementBuilder.AppendLine("(");
        mergeStatementBuilder.AppendLine($"SELECT * FROM {temptableName} ORDER BY \"{Constants.AutoGeneratedIndexNumberColumn}\"");
        mergeStatementBuilder.AppendLine(") AS b");
        mergeStatementBuilder.AppendLine("ON 1 = 0");
        mergeStatementBuilder.AppendLine("WHEN NOT MATCHED THEN");
        mergeStatementBuilder.AppendLine($"INSERT ({string.Join(", ", _columnNames.Select(x => $"\"{GetDbColumnName(x)}\""))})");
        mergeStatementBuilder.AppendLine($"VALUES ({string.Join(", ", _columnNames.Select(x => $"b.\"{x}\""))})");
        mergeStatementBuilder.AppendLine($"RETURNING a.\"{GetDbColumnName(_outputIdColumn)}\", b.\"{Constants.AutoGeneratedIndexNumberColumn}\"");
        mergeStatementBuilder.AppendLine(";");

        await _connection.EnsureOpenAsync(cancellationToken);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");
        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            await createTemptableCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        await data.SqlBulkCopyAsync(temptableName, _columnNames, null, true, _connection, _transaction, _options, valueConverters: _valueConverters, cancellationToken: cancellationToken);
        Log("End executing SqlBulkCopy.");

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connection.CreateTextCommand(_transaction, sqlMergeStatement, _options))
        {
            using var reader = await updateCommand.ExecuteReaderAsync(cancellationToken);
            var dbColumn = GetDbColumnName(_outputIdColumn);
            while (await reader.ReadAsync(cancellationToken))
            {
                returnedIds[(reader[Constants.AutoGeneratedIndexNumberColumn] as long?).Value] = reader[dbColumn];
            }
        }
        Log("End merging temp table.");

        long idx = 0;
        foreach (var row in data)
        {
            idProperty.SetValue(row, returnedIds[idx]);
            idx++;
        }
    }

    public async Task SingleInsertAsync(T dataToInsert, CancellationToken cancellationToken = default)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else if (ReturnGeneratedId && _outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);
            setId(dataToInsert, SequentialGuidGenerator.Next());

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

            if (ReturnGeneratedId)
            {
                insertStatementBuilder.AppendLine($"RETURNING \"{GetDbColumnName(_outputIdColumn)}\"");
            }

        }

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);
        _table.CreateSqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        await _connection.EnsureOpenAsync(cancellationToken);

        if (_options.KeepIdentity || !ReturnGeneratedId)
        {
            var affectedRow = await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            var dbColumn = GetDbColumnName(_outputIdColumn);
            var idProperty = GetIdProperty();

            using var reader = await insertCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var returnedId = reader[dbColumn];

                idProperty.SetValue(dataToInsert, returnedId);
                break;
            }
        }

        Log($"End inserting.");
    }
}
