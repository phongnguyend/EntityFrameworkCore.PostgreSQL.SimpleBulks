using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class ObjectExtensions
{
    public static List<NpgsqlParameter> ToSqlParameters<T>(this T data, IEnumerable<string> propertyNames, IReadOnlyDictionary<string, ValueConverter> valueConverters)
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
            var type = GetProviderClrType(prop, valueConverters);

            var value = GetProviderValue(prop, data, valueConverters);

            var para = new NpgsqlParameter($"@{prop.Name}", value ?? DBNull.Value);

            if (type == typeof(DateTime))
            {
                para.DbType = System.Data.DbType.DateTime2;
            }

            parameters.Add(para);
        }

        return parameters;
    }

    private static Type GetProviderClrType(PropertyDescriptor property, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ProviderClrType;
        }

        return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    }

    private static object GetProviderValue<T>(PropertyDescriptor property, T item, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ConvertToProvider(property.GetValue(item));
        }

        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var tempValue = property.GetValue(item);
        var value = type.IsEnum && tempValue != null ? (int)tempValue : tempValue;

        return value;
    }
}
