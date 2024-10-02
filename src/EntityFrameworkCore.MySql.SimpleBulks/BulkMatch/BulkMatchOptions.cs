namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;

public class BulkMatchOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
