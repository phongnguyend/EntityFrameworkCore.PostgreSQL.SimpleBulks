using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class DateTimeExtensions
{
    public static DateTime TruncateToMicroseconds(this DateTime dateTime)
    {
        long ticks = dateTime.Ticks - (dateTime.Ticks % 10);
        return new DateTime(ticks, dateTime.Kind);
    }

    public static DateTime? TruncateToMicroseconds(this DateTime? dateTime)
    {
        return dateTime?.TruncateToMicroseconds();
    }

    public static DateTimeOffset TruncateToMicroseconds(this DateTimeOffset dateTimeOffset)
    {
        long ticks = dateTimeOffset.Ticks - (dateTimeOffset.Ticks % 10);
        return new DateTimeOffset(ticks, dateTimeOffset.Offset);
    }

    public static DateTimeOffset? TruncateToMicroseconds(this DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset?.TruncateToMicroseconds();
    }
}
