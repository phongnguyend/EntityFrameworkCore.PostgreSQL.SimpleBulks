﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class DbContextExtensions
{
    public static TableInfor GetTableInfor(this DbContext dbContext, Type type)
    {
        var entityType = dbContext.Model.FindEntityType(type);

        var schema = entityType.GetSchema();
        var tableName = entityType.GetTableName();

        return new TableInfor(schema, tableName);
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

    public static IList<ColumnInfor> GetProperties(this DbContext dbContext, Type type)
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
                IsPrimaryKey = entityProp.IsPrimaryKey()
            });

        return data.ToList();
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
}
