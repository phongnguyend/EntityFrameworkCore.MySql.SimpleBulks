namespace EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

public class BulkUpdateOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
