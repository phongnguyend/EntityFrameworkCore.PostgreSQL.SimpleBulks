namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.TempTable;

public class TempTableOptions : BulkOptions
{
    public string TableName { get; set; }

    public string PrefixName { get; set; }
}
