namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMatch
{
    public class BulkMatchOptions : BulkOptions
    {
        public string Collation { get; set; } = Constants.DefaultCollation;
    }
}
