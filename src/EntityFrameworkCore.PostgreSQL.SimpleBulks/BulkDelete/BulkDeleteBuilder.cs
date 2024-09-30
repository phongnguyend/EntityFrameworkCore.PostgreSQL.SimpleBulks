using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete
{
    public class BulkDeleteBuilder<T>
    {
        private TableInfor _table;
        private IEnumerable<string> _idColumns;
        private IDictionary<string, string> _dbColumnMappings;
        private BulkDeleteOptions _options;
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public BulkDeleteBuilder(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public BulkDeleteBuilder(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkDeleteBuilder<T> ToTable(TableInfor table)
        {
            _table = table;
            return this;
        }

        public BulkDeleteBuilder<T> WithId(string idColumn)
        {
            _idColumns = [idColumn];
            return this;
        }

        public BulkDeleteBuilder<T> WithId(IEnumerable<string> idColumns)
        {
            _idColumns = idColumns;
            return this;
        }

        public BulkDeleteBuilder<T> WithId(Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            return this;
        }

        public BulkDeleteBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
            return this;
        }

        public BulkDeleteBuilder<T> ConfigureBulkOptions(Action<BulkDeleteOptions> configureOptions)
        {
            _options = new BulkDeleteOptions();
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

        public BulkDeleteResult Execute(IEnumerable<T> data)
        {
            if (data.Count() == 1)
            {
                return SingleDelete(data.First());
            }

            var temptableName = $"\"{Guid.NewGuid()}\"";
            var clrTypes = typeof(T).GetClrTypes(_idColumns);
            var sqlCreateTemptable = typeof(T).GenerateTempTableDefinition(temptableName, _idColumns);

            var joinCondition = string.Join(" AND ", _idColumns.Select(x =>
            {
                string collation = !string.IsNullOrEmpty(_options.Collation) && clrTypes[x] == typeof(string) ?
                $" COLLATE \"{_options.Collation}\"" : string.Empty;
                return $"a.\"{GetDbColumnName(x)}\"{collation} = b.\"{x}\"{collation}";
            }));

            var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} AS a USING {temptableName} AS b WHERE " + joinCondition;

            _connection.EnsureOpen();

            Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            Log("End creating temp table.");


            Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

            data.SqlBulkCopy(temptableName, _idColumns, null, false, _connection, _transaction, _options);

            Log("End executing SqlBulkCopy.");

            Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

            using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

            var affectedRows = deleteCommand.ExecuteNonQuery();

            Log("End deleting.");

            return new BulkDeleteResult
            {
                AffectedRows = affectedRows
            };
        }

        public BulkDeleteResult SingleDelete(T dataToDelete)
        {
            var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
            {
                return $"\"{GetDbColumnName(x)}\" = @{x}";
            }));

            var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

            Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

            using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

            dataToDelete.ToSqlParameters(_idColumns).ForEach(x => deleteCommand.Parameters.Add(x));

            var affectedRows = deleteCommand.ExecuteNonQuery();

            Log("End deleting.");

            return new BulkDeleteResult
            {
                AffectedRows = affectedRows
            };
        }

        private void Log(string message)
        {
            _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkDelete]: {message}");
        }
    }
}
