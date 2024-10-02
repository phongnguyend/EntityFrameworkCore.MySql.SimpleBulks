namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public class BulkMergeOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
