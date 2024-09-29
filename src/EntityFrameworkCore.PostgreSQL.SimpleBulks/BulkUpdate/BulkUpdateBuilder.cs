﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate
{
    public class BulkUpdateBuilder<T>
    {
        private IEnumerable<T> _data;
        private string _tableName;
        private IEnumerable<string> _idColumns;
        private IEnumerable<string> _columnNames;
        private IDictionary<string, string> _dbColumnMappings;
        private BulkUpdateOptions _options;
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public BulkUpdateBuilder(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public BulkUpdateBuilder(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkUpdateBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public BulkUpdateBuilder<T> ToTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkUpdateBuilder<T> WithId(string idColumn)
        {
            _idColumns = new[] { idColumn };
            return this;
        }

        public BulkUpdateBuilder<T> WithId(IEnumerable<string> idColumns)
        {
            _idColumns = idColumns;
            return this;
        }

        public BulkUpdateBuilder<T> WithId(Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            return this;
        }

        public BulkUpdateBuilder<T> WithColumns(IEnumerable<string> columnNames)
        {
            _columnNames = columnNames;
            return this;
        }

        public BulkUpdateBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
        {
            _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public BulkUpdateBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
            return this;
        }

        public BulkUpdateBuilder<T> ConfigureBulkOptions(Action<BulkUpdateOptions> configureOptions)
        {
            _options = new BulkUpdateOptions();
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

        public BulkUpdateResult Execute()
        {
            if (_data.Count() == 1)
            {
                return SingleUpdate(_data.First());
            }

            var temptableName = $"\"{Guid.NewGuid()}\"";

            var propertyNamesIncludeId = _columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.AddRange(_idColumns);

            var clrTypes = typeof(T).GetClrTypes(propertyNamesIncludeId);
            var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, propertyNamesIncludeId);

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = !string.IsNullOrEmpty(_options.Collation) && clrTypes[x] == typeof(string) ?
                $" COLLATE \"{_options.Collation}\"" : string.Empty;
                return $"a.\"{GetDbColumnName(x)}\"{collation} = b.\"{x}\"{collation}";
            }));

            var updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine($"UPDATE {_tableName} AS a SET");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _columnNames.Select(x => CreateSetStatement(x, "b"))));
            updateStatementBuilder.AppendLine($"FROM {temptableName} AS b WHERE " + joinCondition);

            _connection.EnsureOpen();

            Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");
            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
            {
                createTemptableCommand.ExecuteNonQuery();
            }
            Log("End creating temp table.");

            Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
            _data.SqlBulkCopy(temptableName, propertyNamesIncludeId, null, false, _connection, _transaction, _options);
            Log("End executing SqlBulkCopy.");

            var sqlUpdateStatement = updateStatementBuilder.ToString();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");
            using var updateCommand = _connection.CreateTextCommand(_transaction, sqlUpdateStatement, _options);
            var affectedRows = updateCommand.ExecuteNonQuery();
            Log("End updating.");

            return new BulkUpdateResult
            {
                AffectedRows = affectedRows
            };
        }

        public BulkUpdateResult SingleUpdate(T dataToUpdate)
        {
            var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
            {
                return CreateSetStatement(x);
            }));

            var updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine($"UPDATE {_tableName} SET");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _columnNames.Select(x => CreateSetStatement(x))));
            updateStatementBuilder.AppendLine($"WHERE {whereCondition}");

            var sqlUpdateStatement = updateStatementBuilder.ToString();

            var propertyNamesIncludeId = _columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.AddRange(_idColumns);

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connection.CreateTextCommand(_transaction, sqlUpdateStatement, _options);

            dataToUpdate.ToSqlParameters(propertyNamesIncludeId).ForEach(x => updateCommand.Parameters.Add(x));

            _connection.EnsureOpen();

            var affectedRow = updateCommand.ExecuteNonQuery();

            Log($"End updating.");

            return new BulkUpdateResult
            {
                AffectedRows = affectedRow
            };
        }

        private string CreateSetStatement(string prop, string rightTable)
        {
            string sqlOperator = "=";
            string sqlProp = RemoveOperator(prop);

            if (prop.EndsWith("+="))
            {
                sqlOperator = "+=";
            }

            return $"\"{GetDbColumnName(sqlProp)}\" {sqlOperator} {rightTable}.\"{sqlProp}\"";
        }

        private string CreateSetStatement(string prop)
        {
            string sqlOperator = "=";
            string sqlProp = RemoveOperator(prop);

            if (prop.EndsWith("+="))
            {
                sqlOperator = "+=";
            }

            return $"\"{GetDbColumnName(sqlProp)}\" {sqlOperator} @{sqlProp}";
        }

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }

        private void Log(string message)
        {
            _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkUpdate]: {message}");
        }
    }
}
