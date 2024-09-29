﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert
{
    public class BulkInsertBuilder<T>
    {
        private IEnumerable<T> _data;
        private string _tableName;
        private string _outputIdColumn;
        private IEnumerable<string> _columnNames;
        private IDictionary<string, string> _dbColumnMappings;
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

        public BulkInsertBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public BulkInsertBuilder<T> ToTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        [Obsolete("Typo Issue, Shoud use WithOutputId")]
        public BulkInsertBuilder<T> WithOuputId(string idColumn)
        {
            return WithOutputId(idColumn);
        }

        public BulkInsertBuilder<T> WithOutputId(string idColumn)
        {
            _outputIdColumn = idColumn;
            return this;
        }

        [Obsolete("Typo Issue, Shoud use WithOutputId")]
        public BulkInsertBuilder<T> WithOuputId(Expression<Func<T, object>> idSelector)
        {
            return WithOutputId(idSelector);
        }

        public BulkInsertBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
        {
            _outputIdColumn = idSelector.Body.GetMemberName();
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

        public BulkInsertBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
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
            if (_dbColumnMappings == null)
            {
                return columnName;
            }

            return _dbColumnMappings.ContainsKey(columnName) ? _dbColumnMappings[columnName] : columnName;
        }

        public void Execute()
        {
            if (_data.Count() == 1)
            {
                SingleInsert(_data.First());
                return;
            }

            if (string.IsNullOrWhiteSpace(_outputIdColumn))
            {
                _connection.EnsureOpen();

                Log($"Begin executing SqlBulkCopy. TableName: {_tableName}");
                _data.SqlBulkCopy(_tableName, _columnNames, _dbColumnMappings, false, _connection, _transaction, _options);
                Log("End executing SqlBulkCopy.");
                return;
            }

            if (_options.KeepIdentity)
            {
                var columns = _columnNames.Select(x => x).ToList();
                if (!columns.Contains(_outputIdColumn))
                {
                    columns.Add(_outputIdColumn);
                }

                _connection.EnsureOpen();

                Log($"Begin executing SqlBulkCopy. TableName: {_tableName}");
                _data.SqlBulkCopy(_tableName, columns, _dbColumnMappings, false, _connection, _transaction, _options);
                Log("End executing SqlBulkCopy.");
                return;
            }

            var temptableName = $"\"{Guid.NewGuid()}\"";
            var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, _columnNames, addIndexNumberColumn: true);

            var mergeStatementBuilder = new StringBuilder();
            mergeStatementBuilder.AppendLine($"MERGE INTO {_tableName} AS a");
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
            _data.SqlBulkCopy(temptableName, _columnNames, null, true, _connection, _transaction, _options);
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

            var idProperty = typeof(T).GetProperty(_outputIdColumn);

            long idx = 0;
            foreach (var row in _data)
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

                insertStatementBuilder.AppendLine($"INSERT INTO {_tableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
                insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
            }
            else
            {
                insertStatementBuilder.AppendLine($"INSERT INTO {_tableName} ({string.Join(", ", columnsToInsert.Select(x => $"\"{GetDbColumnName(x)}\""))})");
                insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

                if (!string.IsNullOrEmpty(_outputIdColumn))
                {
                    insertStatementBuilder.AppendLine($"RETURNING \"{GetDbColumnName(_outputIdColumn)}\"");
                }

            }

            var insertStatement = insertStatementBuilder.ToString();

            using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);
            dataToInsert.ToSqlParameters(columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

            Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

            _connection.EnsureOpen();

            if (_options.KeepIdentity || string.IsNullOrEmpty(_outputIdColumn))
            {
                var affectedRow = insertCommand.ExecuteNonQuery();
            }
            else
            {
                var dbColumn = GetDbColumnName(_outputIdColumn);
                var idProperty = typeof(T).GetProperty(_outputIdColumn);

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
    }
}
