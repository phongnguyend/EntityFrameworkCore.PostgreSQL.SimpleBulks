using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public class ValueConverter
{
    public Type ProviderClrType { get; init; }

    public Func<object?, object?> ConvertToProvider { get; init; }

    public Func<object?, object?> ConvertFromProvider { get; init; }
}
