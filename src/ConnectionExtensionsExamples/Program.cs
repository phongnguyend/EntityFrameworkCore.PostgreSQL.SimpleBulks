using ConnectionExtensionsExamples;
using ConnectionExtensionsExamples.Entities;
using EntityFrameworkCore.PostgreSQL.SimpleBulks;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.DirectUpdate;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

TableMapper.Register(new NpgsqlTableInfor<ConfigurationEntry>("ConfigurationEntries")
{
    PrimaryKeys = ["Id"],
    OutputId = new OutputId
    {
        Name = "Id",
        Mode = OutputIdMode.ServerGenerated,
    }
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var existingConfigurationEntries = new List<ConfigurationEntry>();

// Use DbContext to create the database and apply migrations only.
using (var dbct = new DemoDbContext())
{
    dbct.Database.Migrate();

    existingConfigurationEntries = dbct.Set<ConfigurationEntry>().AsNoTracking().ToList();
}

var connection = new ConnectionContext(new NpgsqlConnection(ConnectionStrings.PostgreSQLConnectionString), null);

var deleteResult = await connection.BulkDeleteAsync(existingConfigurationEntries,
    options: new BulkDeleteOptions
    {
        LogTo = Console.WriteLine
    });

Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");

var configurationEntries = new List<ConfigurationEntry>();

for (int i = 0; i < 1000; i++)
{
    configurationEntries.Add(new ConfigurationEntry
    {
        Key = $"Key{i}",
        Value = $"Value{i}",
        CreatedDateTime = DateTimeOffset.Now,
        SeasonAsInt = Season.Spring,
        SeasonAsString = Season.Spring,
    });
}

await connection.BulkInsertAsync(configurationEntries,
    x => new { x.Key, x.Value, x.CreatedDateTime, x.UpdatedDateTime, x.IsSensitive, x.Description, x.SeasonAsInt, x.SeasonAsString },
    options: new BulkInsertOptions
    {
        LogTo = Console.WriteLine
    });

foreach (var row in configurationEntries)
{
    row.Key += "xx";
    row.UpdatedDateTime = DateTimeOffset.Now;
    row.SeasonAsInt = Season.Winter;
    row.SeasonAsString = Season.Winter;
    row.IsSensitive = true;
    row.Description = row.Id.ToString();
}

var updateResult = await connection.BulkUpdateAsync(configurationEntries,
    x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description, x.SeasonAsInt, x.SeasonAsString },
    options: new BulkUpdateOptions
    {
        LogTo = Console.WriteLine
    });

Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");

configurationEntries.Add(new ConfigurationEntry
{
    Key = $"Key{1001}",
    Value = $"Value{1001}",
    CreatedDateTime = DateTimeOffset.Now,
    SeasonAsInt = Season.Summer,
    SeasonAsString = Season.Summer,
});

var mergeResult = await connection.BulkMergeAsync(configurationEntries,
    x => x.Id,
    x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description },
    x => new { x.Key, x.Value, x.IsSensitive, x.CreatedDateTime, x.SeasonAsInt, x.SeasonAsString },
    options: new BulkMergeOptions
    {
        LogTo = Console.WriteLine
    });

Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");

var configurationEntry = new ConfigurationEntry
{
    Key = $"Key_DirectInsert",
    Value = $"Value_DirectInsert",
    CreatedDateTime = DateTimeOffset.Now,
    SeasonAsInt = Season.Autumn,
    SeasonAsString = Season.Autumn,
};

await connection.DirectInsertAsync(configurationEntry,
    x => new { x.Key, x.Value, x.CreatedDateTime, x.UpdatedDateTime, x.IsSensitive, x.Description, x.SeasonAsInt, x.SeasonAsString },
    options: new BulkInsertOptions
    {
        LogTo = Console.WriteLine
    });

configurationEntry.Key += "_Updated";
configurationEntry.Value += "_Updated";
configurationEntry.UpdatedDateTime = DateTimeOffset.Now;
configurationEntry.SeasonAsInt = Season.Spring;
configurationEntry.SeasonAsString = Season.Spring;

await connection.DirectUpdateAsync(configurationEntry,
    x => new { x.Key, x.Value, x.UpdatedDateTime, x.SeasonAsInt, x.SeasonAsString },
    options: new BulkUpdateOptions
    {
        LogTo = Console.WriteLine
    });

await connection.DirectDeleteAsync(configurationEntry,
    options: new BulkDeleteOptions
    {
        LogTo = Console.WriteLine
    });

Console.WriteLine("Finished!");
Console.ReadLine();