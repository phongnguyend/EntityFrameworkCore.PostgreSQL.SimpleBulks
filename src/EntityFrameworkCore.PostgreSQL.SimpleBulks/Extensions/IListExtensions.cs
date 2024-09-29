﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions
{
    public static class IListExtensions
    {
        public static void SqlBulkCopy<T>(this IEnumerable<T> data, string tableName, IEnumerable<string> propertyNames, IDictionary<string, string> dbColumnMappings, bool addIndexNumberColumn, NpgsqlConnection connection, NpgsqlTransaction transaction, BulkOptions options = null)
        {
            options ??= new BulkOptions()
            {
                BatchSize = 0,
                Timeout = 30,
            };

            var properties = TypeDescriptor.GetProperties(typeof(T));

            var updatablePros = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor prop in properties)
            {
                if (propertyNames.Contains(prop.Name))
                {
                    updatablePros.Add(prop);
                }
            }

            var columnNames = propertyNames.ToList();

            if (addIndexNumberColumn)
            {
                columnNames.Add(Constants.AutoGeneratedIndexNumberColumn);
            }

            var sql = $"COPY {tableName} ({string.Join(',', columnNames.Select(x => $"\"{GetDbColumnName(x, dbColumnMappings)}\""))}) FROM STDIN (FORMAT binary)";

            using var writer = connection.BeginBinaryImport(sql);

            long idx = 0;

            foreach (T item in data)
            {
                writer.StartRow();

                foreach (var name in columnNames)
                {
                    var prop = updatablePros.FirstOrDefault(x => x.Name == name);

                    if (prop == null)
                    {
                        continue;
                    }

                    var value = prop.GetValue(item);
                    writer.Write(value);
                }

                if (addIndexNumberColumn)
                {
                    writer.Write(idx);
                }

                idx++;
            }

            writer.Complete();
        }

        private static string GetDbColumnName(string columName, IDictionary<string, string> dbColumnMappings)
        {
            if (dbColumnMappings == null)
            {
                return columName;
            }

            return dbColumnMappings.ContainsKey(columName) ? dbColumnMappings[columName] : columName;
        }
    }
}
