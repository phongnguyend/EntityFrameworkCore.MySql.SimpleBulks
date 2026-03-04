using System;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

public class BulkUpdateOptions : BulkOptions
{
    public static readonly BulkUpdateOptions DefaultOptions = new BulkUpdateOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public bool CreateIndexOnTempTable { get; set; } = true;

    public Func<SetClauseContext, string> ConfigureSetClause { get; set; }
}
