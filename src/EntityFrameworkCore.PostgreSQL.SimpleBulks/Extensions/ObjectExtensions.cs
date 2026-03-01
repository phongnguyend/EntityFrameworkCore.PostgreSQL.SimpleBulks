using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class ObjectExtensions
{
    public static List<ParameterInfo> ToNpgsqlParameterInfors(this object parameters)
    {
        if (parameters == null)
        {
            return [];
        }

        var result = new List<ParameterInfo>();
        var properties = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(parameters);
            var parameterName = property.Name.StartsWith("@") ? property.Name : $"@{property.Name}";
            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            var parameter = new NpgsqlParameter(parameterName, value ?? DBNull.Value)
            {
                DataTypeName = underlyingType.ToPostgreSQLType()
            };

            result.Add(new ParameterInfo
            {
                Name = parameter.ParameterName,
                Type = parameter.DataTypeName,
                Parameter = parameter
            });
        }

        return result;
    }
}
