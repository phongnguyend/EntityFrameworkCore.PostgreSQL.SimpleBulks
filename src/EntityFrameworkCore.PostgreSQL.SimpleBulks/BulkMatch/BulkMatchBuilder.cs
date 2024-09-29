﻿using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMatch
{
    public class BulkMatchBuilder<T>
    {
        private string _tableName;
        private IEnumerable<string> _matchedColumns;
        private IEnumerable<string> _returnedColumns;
        private IDictionary<string, string> _dbColumnMappings;
        private BulkMatchOptions _options;
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public BulkMatchBuilder(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public BulkMatchBuilder(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkMatchBuilder<T> WithTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkMatchBuilder<T> WithMatchedColumn(string matchedColumn)
        {
            _matchedColumns = new List<string> { matchedColumn };
            return this;
        }

        public BulkMatchBuilder<T> WithMatchedColumns(IEnumerable<string> matchedColumns)
        {
            _matchedColumns = matchedColumns;
            return this;
        }

        public BulkMatchBuilder<T> WithMatchedColumns(Expression<Func<T, object>> matchedColumnsSelector)
        {
            var matchedColumn = matchedColumnsSelector.Body.GetMemberName();
            _matchedColumns = string.IsNullOrEmpty(matchedColumn) ? matchedColumnsSelector.Body.GetMemberNames() : new List<string> { matchedColumn };
            return this;
        }

        public BulkMatchBuilder<T> WithReturnedColumns(IEnumerable<string> returnedColumns)
        {
            _returnedColumns = returnedColumns;
            return this;
        }

        public BulkMatchBuilder<T> WithReturnedColumns(Expression<Func<T, object>> returnedColumnsSelector)
        {
            _returnedColumns = returnedColumnsSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public BulkMatchBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
            return this;
        }

        public BulkMatchBuilder<T> ConfigureBulkOptions(Action<BulkMatchOptions> configureOptions)
        {
            _options = new BulkMatchOptions();
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

        public List<T> Execute(IEnumerable<T> machedValues)
        {
            var temptableName = $"\"{Guid.NewGuid()}\"";

            var clrTypes = typeof(T).GetClrTypes(_matchedColumns);
            var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, _matchedColumns);

            var joinCondition = string.Join(" AND ", _matchedColumns.Select(x =>
            {
                string collation = !string.IsNullOrEmpty(_options.Collation) && clrTypes[x] == typeof(string) ?
                $" COLLATE \"{_options.Collation}\"" : string.Empty;
                return $"a.\"{GetDbColumnName(x)}\"{collation} = b.\"{x}\"{collation}";
            }));

            var selectQueryBuilder = new StringBuilder();
            selectQueryBuilder.AppendLine($"SELECT {string.Join(", ", _returnedColumns.Select(x => CreateSelectStatement(x)))} ");
            selectQueryBuilder.AppendLine($"FROM {_tableName} a JOIN {temptableName} b ON " + joinCondition);

            _connection.EnsureOpen();

            Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            Log("End creating temp table.");


            Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

            machedValues.SqlBulkCopy(temptableName, _matchedColumns, null, false, _connection, _transaction, _options);

            Log("End executing SqlBulkCopy.");

            var selectQuery = selectQueryBuilder.ToString();

            Log($"Begin matching:{Environment.NewLine}{selectQuery}");

            var results = new List<T>();

            var properties = typeof(T).GetProperties().Where(prop => _returnedColumns.Contains(prop.Name)).ToList();

            using var updateCommand = _connection.CreateTextCommand(_transaction, selectQuery, _options);
            using var reader = updateCommand.ExecuteReader();
            while (reader.Read())
            {
                T obj = (T)Activator.CreateInstance(typeof(T));

                foreach (var prop in properties)
                {
                    if (!Equals(reader[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, reader[prop.Name], null);
                    }
                }

                results.Add(obj);
            }

            Log($"End matching.");

            return results;
        }

        private string CreateSelectStatement(string colunmName)
        {
            return $"a.\"{GetDbColumnName(colunmName)}\" as \"{colunmName}\"";
        }

        private void Log(string message)
        {
            _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMatch]: {message}");
        }
    }
}