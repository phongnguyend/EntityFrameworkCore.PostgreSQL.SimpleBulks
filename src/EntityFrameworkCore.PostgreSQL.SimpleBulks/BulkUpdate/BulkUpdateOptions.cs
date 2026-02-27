using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

public class BulkUpdateOptions : BulkOptions
{
    public static readonly BulkUpdateOptions DefaultOptions = new BulkUpdateOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public Func<SetClauseContext, string> ConfigureSetClause { get; set; }
}
