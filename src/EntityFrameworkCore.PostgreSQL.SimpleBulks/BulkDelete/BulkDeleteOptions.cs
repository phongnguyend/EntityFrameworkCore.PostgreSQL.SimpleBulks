namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;

public class BulkDeleteOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
