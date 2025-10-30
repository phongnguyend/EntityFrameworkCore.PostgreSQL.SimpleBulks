namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;

public class BulkMergeOptions : BulkOptions
{
    public static readonly BulkMergeOptions DefaultOptions = new BulkMergeOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public bool ReturnDbGeneratedId { get; set; } = true;
}
