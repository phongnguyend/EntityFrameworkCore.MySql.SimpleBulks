namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;

public class BulkMatchOptions : BulkOptions
{
    public static readonly BulkMatchOptions DefaultOptions = new BulkMatchOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public bool CreateIndexOnTempTable { get; set; } = true;
}
