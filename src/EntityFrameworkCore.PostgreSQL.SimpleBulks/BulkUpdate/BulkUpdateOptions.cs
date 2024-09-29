namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate
{
    public class BulkUpdateOptions : BulkOptions
    {
        public string Collation { get; set; } = Constants.DefaultCollation;
    }
}
