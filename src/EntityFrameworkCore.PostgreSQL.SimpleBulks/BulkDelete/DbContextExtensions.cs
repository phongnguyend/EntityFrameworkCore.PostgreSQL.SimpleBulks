using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete
{
    public static class DbContextExtensions
    {
        public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkDeleteOptions> configureOptions = null)
        {
            var table = dbContext.GetTableInfor(typeof(T));
            var connection = dbContext.GetNpgsqlConnection();
            var transaction = dbContext.GetCurrentNpgsqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var primaryKeys = properties
                .Where(x => x.IsPrimaryKey)
                .Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkDeleteBuilder<T>(connection, transaction)
                 .WithId(primaryKeys)
                 .WithDbColumnMappings(dbColumnMappings)
                 .ToTable(table)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(data);
        }
    }
}
