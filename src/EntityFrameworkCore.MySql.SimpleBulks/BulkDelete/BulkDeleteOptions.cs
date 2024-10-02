namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public class BulkDeleteOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
