namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge
{
    public class BulkMergeResult
    {
        public int AffectedRows { get; set; }

        public int InsertedRows { get; set; }

        public int UpdatedRows { get; set; }
    }
}
