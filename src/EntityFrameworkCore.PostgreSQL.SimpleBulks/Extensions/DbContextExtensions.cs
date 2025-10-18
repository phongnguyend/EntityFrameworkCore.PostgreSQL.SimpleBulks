using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class DbContextExtensions
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<ColumnInfor>> _propertiesCache = [];
    private static readonly ConcurrentDictionary<Type, TableInfor> _tableInfoCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> _columnNamesCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> _columnTypesCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<string>> _primaryKeysCache = [];
    private static readonly ConcurrentDictionary<Type, ColumnInfor> _outputIdCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<string>> _insertablePropertyNamesCache = [];
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<string>> _allPropertyNamesCache = [];

    public static TableInfor GetTableInfor(this DbContext dbContext, Type type)
    {
        return _tableInfoCache.GetOrAdd(type, (type) =>
        {
            var entityType = dbContext.Model.FindEntityType(type);

            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();

            var tableInfo = new TableInfor(schema, tableName);
            return tableInfo;
        });
    }

    public static bool IsEntityType(this DbContext dbContext, Type type)
    {
        return dbContext.Model.FindEntityType(type) != null;
    }

    public static NpgsqlConnection GetNpgsqlConnection(this DbContext dbContext)
    {
        return dbContext.Database.GetDbConnection().AsNpgsqlConnection();
    }

    public static NpgsqlTransaction GetCurrentNpgsqlTransaction(this DbContext dbContext)
    {
        var transaction = dbContext.Database.CurrentTransaction;
        return transaction == null ? null : transaction.GetDbTransaction() as NpgsqlTransaction;
    }

    public static IReadOnlyList<ColumnInfor> GetProperties(this DbContext dbContext, Type type)
    {
        return _propertiesCache.GetOrAdd(type, (type) =>
        {
            var typeProperties = type.GetProperties().Select(x => new { x.Name, x.PropertyType });
            var entityProperties = dbContext.Model.FindEntityType(type)
                           .GetProperties();

            var data = typeProperties.Join(entityProperties,
                prop => prop.Name,
                entityProp => entityProp.Name,
                (prop, entityProp) => new ColumnInfor
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.PropertyType,
                    ColumnName = entityProp.GetColumnName(),
                    ColumnType = entityProp.GetColumnType(),
                    ValueGenerated = entityProp.ValueGenerated,
                    DefaultValueSql = entityProp.GetDefaultValueSql(),
                    IsPrimaryKey = entityProp.IsPrimaryKey(),
                    IsRowVersion = entityProp.IsRowVersion()
                }).ToArray();
            return data;
        });
    }

    public static IReadOnlyDictionary<string, string> GetColumnNames(this DbContext dbContext, Type type)
    {
        return _columnNamesCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
        });
    }

    public static IReadOnlyDictionary<string, string> GetColumnTypes(this DbContext dbContext, Type type)
    {
        return _columnTypesCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.ToDictionary(x => x.PropertyName, x => x.ColumnType);
        });
    }

    public static IReadOnlyList<string> GetPrimaryKeys(this DbContext dbContext, Type type)
    {
        return _primaryKeysCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.Where(x => x.IsPrimaryKey).Select(x => x.PropertyName).ToArray();
        });
    }

    public static ColumnInfor GetOutputId(this DbContext dbContext, Type type)
    {
        return _outputIdCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd).FirstOrDefault();
        });
    }

    public static IReadOnlyList<string> GetInsertablePropertyNames(this DbContext dbContext, Type type)
    {
        return _insertablePropertyNamesCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.Where(x => x.ValueGenerated == ValueGenerated.Never).Select(x => x.PropertyName).ToArray();
        });
    }

    public static IReadOnlyList<string> GetAllPropertyNames(this DbContext dbContext, Type type)
    {
        return _allPropertyNamesCache.GetOrAdd(type, (type) =>
        {
            var properties = dbContext.GetProperties(type);
            return properties.Select(x => x.PropertyName).ToArray();
        });
    }

    public static IDbCommand CreateTextCommand(this DbContext dbContext, string commandText, BulkOptions options = null)
    {
        return dbContext.GetNpgsqlConnection().CreateTextCommand(dbContext.GetCurrentNpgsqlTransaction(), commandText, options);
    }

    public static void ExecuteReader(this DbContext dbContext, string commandText, Action<IDataReader> action, BulkOptions options = null)
    {
        using var updateCommand = dbContext.CreateTextCommand(commandText, options);
        using var reader = updateCommand.ExecuteReader();

        while (reader.Read())
        {
            action(reader);
        }
    }

    public static List<Guid> GenerateDbSequentialIds(this DbContext dbContext, int count)
    {
        var query = string.Join($"UNION ALL{Environment.NewLine}", Enumerable.Range(0, count).Select(x => $"select uuid_generate_v1mc(){Environment.NewLine}"));

        return dbContext.Database.SqlQueryRaw<Guid>(query).ToList();
    }

    public static void GenerateDbSequentialIds(this DbContext dbContext, Queue<Guid> guids, int count)
    {
        foreach (var item in GenerateDbSequentialIds(dbContext, count))
        {
            guids.Enqueue(item);
        }
    }

    public static bool IsRowVersion(this IProperty property)
    {
        return property.IsConcurrencyToken
            && property.ValueGenerated == ValueGenerated.OnAddOrUpdate
            && property.ClrType == typeof(uint)
            && string.Equals(property.GetColumnName(), "xmin", StringComparison.OrdinalIgnoreCase)
            && string.Equals(property.GetColumnType(), "xid", StringComparison.OrdinalIgnoreCase);
    }
}
