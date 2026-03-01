using System;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public class BulkMergeOptions : BulkOptions
{
    public static readonly BulkMergeOptions DefaultOptions = new BulkMergeOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public Func<SetClauseContext, string> ConfigureSetClause { get; set; }

    public Func<MergeContext, WhenNotMatchedBySourceAction> ConfigureWhenNotMatchedBySource { get; set; }
}

public record struct MergeContext
{
    public TableInfor TableInfor { get; set; }

    public string TargetTableAlias { get; set; }

    public string SourceTableAlias { get; set; }

    private string Quote(string name)
    {
        return $"{Constants.BeginQuote}{name}{Constants.EndQuote}";
    }

    public string GetTargetTableColumnWithoutAlias(string propertyName)
    {
        var columnName = TableInfor.GetDbColumnName(propertyName);

        return $"{Quote(columnName)}";
    }

    public string GetTargetTableColumnWithAlias(string propertyName)
    {
        var columnName = TableInfor.GetDbColumnName(propertyName);

        return string.IsNullOrEmpty(TargetTableAlias) ? $"{Quote(columnName)}" : $"{TargetTableAlias}.{Quote(columnName)}";
    }

    public string GetSourceTableColumn(string propertyName)
    {
        return string.IsNullOrEmpty(SourceTableAlias) ? TableInfor.CreateParameterName(propertyName) : $"{SourceTableAlias}.{Quote(propertyName)}";
    }
}

public enum WhenNotMatchedBySourceActionType
{
    Delete,
    Update
}

public record struct WhenNotMatchedBySourceAction
{
    public WhenNotMatchedBySourceActionType ActionType { get; set; }

    public string AndCondition { get; set; }

    public string SetClause { get; set; }

    public object Parameters { get; set; }
}
