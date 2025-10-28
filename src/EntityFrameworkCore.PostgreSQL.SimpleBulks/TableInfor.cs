﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public abstract class TableInfor
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public TableInfor(string schema, string tableName)
    {
        Schema = schema;
        Name = tableName;

        SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
    }

    public TableInfor(string tableName) : this(null, tableName)
    {
    }

    public abstract List<NpgsqlParameter> CreateSqlParameters<T>(NpgsqlCommand command, T data, IEnumerable<string> propertyNames);
}

public class DbContextTableInfor : TableInfor
{
    private readonly DbContext _dbContext;

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }

    public DbContextTableInfor(string schema, string tableName, DbContext dbContext) : base(schema, tableName)
    {
        _dbContext = dbContext;
    }

    public DbContextTableInfor(string tableName, DbContext dbContext) : base(tableName)
    {
        _dbContext = dbContext;
    }

    public override List<NpgsqlParameter> CreateSqlParameters<T>(NpgsqlCommand command, T data, IEnumerable<string> propertyNames)
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        var parameters = new List<NpgsqlParameter>();

        var mappingSource = _dbContext.GetService<IRelationalTypeMappingSource>();

        foreach (PropertyDescriptor prop in updatablePros)
        {
            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(prop.Name, out var columnType))
            {
                var mapping = mappingSource.FindMapping(columnType);
                var para = (NpgsqlParameter)mapping.CreateParameter(command, prop.Name, GetProviderValue(prop, data) ?? DBNull.Value);
                parameters.Add(para);
            }
        }

        return parameters;

    }

    private object GetProviderValue<T>(PropertyDescriptor property, T item)
    {
        if (ValueConverters != null && ValueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ConvertToProvider(property.GetValue(item));
        }

        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var tempValue = property.GetValue(item);
        var value = type.IsEnum && tempValue != null ? (int)tempValue : tempValue;

        return value;
    }
}

public class NpgsqlTableInfor : TableInfor
{
    public NpgsqlTableInfor(string schema, string tableName) : base(schema, tableName)
    {
    }

    public NpgsqlTableInfor(string tableName) : base(tableName)
    {
    }

    public override List<NpgsqlParameter> CreateSqlParameters<T>(NpgsqlCommand command, T data, IEnumerable<string> propertyNames)
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        var parameters = new List<NpgsqlParameter>();

        foreach (PropertyDescriptor prop in updatablePros)
        {
            var value = GetProviderValue(prop, data);

            var para = new NpgsqlParameter($"@{prop.Name}", value ?? DBNull.Value);

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (type == typeof(DateTime))
            {
                para.DbType = System.Data.DbType.DateTime2;
            }
            else if (type == typeof(DateTimeOffset))
            {
                para.DbType = System.Data.DbType.DateTimeOffset;
            }

            parameters.Add(para);
        }

        return parameters;

    }

    private static object GetProviderValue<T>(PropertyDescriptor property, T item)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var tempValue = property.GetValue(item);
        var value = type.IsEnum && tempValue != null ? (int)tempValue : tempValue;

        return value;
    }
}
