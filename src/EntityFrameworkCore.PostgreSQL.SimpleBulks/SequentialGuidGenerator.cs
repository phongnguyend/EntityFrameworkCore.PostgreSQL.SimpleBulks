using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

/// <summary>
/// http://github.com/npgsql/efcore.pg/blob/main/src/EFCore.PG/ValueGeneration/NpgsqlSequentialGuidValueGenerator.cs
/// </summary>
public static class SequentialGuidGenerator
{
    public static Guid Next()
    {
        return Next(DateTimeOffset.UtcNow);
    }

    public static Guid Next(DateTimeOffset timeNow)
    {
        return Guid.CreateVersion7(timeNow);
    }
}