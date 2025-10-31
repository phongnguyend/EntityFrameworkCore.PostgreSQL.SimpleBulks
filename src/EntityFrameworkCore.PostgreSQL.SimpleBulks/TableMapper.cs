using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public static class TableMapper
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<Type, TableInfor> _mappings = new Dictionary<Type, TableInfor>();

    public static void Register(Type type, NpgsqlTableInfor tableInfo)
    {
        lock (_lock)
        {
            _mappings[type] = tableInfo;
        }
    }

    public static TableInfor Resolve(Type type)
    {
        if (!_mappings.TryGetValue(type, out var tableInfo))
        {
            throw new Exception($"Type {type} has not been registered.");
        }

        return tableInfo;
    }
}
