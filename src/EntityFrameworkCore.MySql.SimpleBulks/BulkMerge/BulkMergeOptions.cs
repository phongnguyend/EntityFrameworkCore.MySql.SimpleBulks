﻿namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public class BulkMergeOptions : BulkOptions
{
    public static readonly BulkMergeOptions DefaultOptions = new BulkMergeOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;
}
