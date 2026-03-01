using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public class BulkMergeBuilder<T>
{
    private TableInfor<T> _table;
    private IReadOnlyCollection<string> _mergeKeys;
    private IReadOnlyCollection<string> _updateColumnNames;
    private IReadOnlyCollection<string> _insertColumnNames;
    private string _outputIdColumn;
    private BulkMergeOptions _options = BulkMergeOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkMergeBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkMergeBuilder<T> ToTable(TableInfor<T> table)
    {
        _table = table;

        _outputIdColumn = table?.OutputId?.Name;

        return this;
    }

    public BulkMergeBuilder<T> WithId(IReadOnlyCollection<string> idColumns)
    {
        _mergeKeys = idColumns;
        return this;
    }

    public BulkMergeBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _mergeKeys = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : [idColumn];
        return this;
    }

    public BulkMergeBuilder<T> WithUpdateColumns(IReadOnlyCollection<string> updateColumnNames)
    {
        _updateColumnNames = updateColumnNames;
        return this;
    }

    public BulkMergeBuilder<T> WithUpdateColumns(Expression<Func<T, object>> updateColumnNamesSelector)
    {
        _updateColumnNames = updateColumnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkMergeBuilder<T> WithInsertColumns(IReadOnlyCollection<string> insertColumnNames)
    {
        _insertColumnNames = insertColumnNames;
        return this;
    }

    public BulkMergeBuilder<T> WithInsertColumns(Expression<Func<T, object>> insertColumnNamesSelector)
    {
        _insertColumnNames = insertColumnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkMergeBuilder<T> WithBulkOptions(BulkMergeOptions options)
    {
        _options = options ?? BulkMergeOptions.DefaultOptions;
        return this;
    }

    private IReadOnlyCollection<string> GetKeys()
    {
        return _table.IncludeDiscriminator(_mergeKeys);
    }

    private string CreateJoinCondition(System.Data.DataTable dataTable)
    {
        return string.Join(" and ", GetKeys().Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" collate {_options.Collation}" : string.Empty;
            return $"s.`{x}`{collation} = t.`{_table.GetDbColumnName(x)}`{collation}";
        }));
    }

    private string CreateWhereIsNullCondition()
    {
        return string.Join(" and ", GetKeys().Select(x =>
        {
            return $"t.`{_table.GetDbColumnName(x)}` IS NULL";
        }));
    }

    private string CreateWhereCondition()
    {
        return string.Join(" AND ", GetKeys().Select(x =>
        {
            return CreateWhereStatement(x);
        }));
    }

    private string CreateWhereNotExistsCondition()
    {
        return $"SELECT 1 FROM {_table.SchemaQualifiedTableName} WHERE " + string.Join(" AND ", GetKeys().Select(x =>
        {
            return CreateWhereStatement(x);
        }));
    }

    private string CreateIndex(string tableName)
    {
        return $"CREATE INDEX Idx_Id ON {tableName} ({string.Join(",", GetKeys().Select(x => $"`{x}`"))});";
    }

    private WhenNotMatchedBySourceAction? GetWhenNotMatchedBySourceAction(string targetTableAlias, string sourceTableAlias)
    {
        if (_options?.ConfigureWhenNotMatchedBySource == null)
        {
            return null;
        }

        var context = new MergeContext
        {
            TableInfor = _table,
            TargetTableAlias = targetTableAlias,
            SourceTableAlias = sourceTableAlias
        };

        return _options.ConfigureWhenNotMatchedBySource(context);
    }

    private static bool HasWhenNotMatchedBySourceAction(WhenNotMatchedBySourceAction? action)
    {
        if (!action.HasValue)
        {
            return false;
        }

        var value = action.Value;
        if (value.ActionType == WhenNotMatchedBySourceActionType.Update && string.IsNullOrEmpty(value.SetClause))
        {
            return false;
        }

        return true;
    }

    private string BuildNotMatchedBySourceStatement(WhenNotMatchedBySourceAction action, string tableAlias, string whereClause)
    {
        var builder = new StringBuilder();

        if (action.ActionType == WhenNotMatchedBySourceActionType.Delete)
        {
            builder.AppendLine($"DELETE FROM {_table.SchemaQualifiedTableName} {tableAlias}".TrimEnd());
        }
        else if (action.ActionType == WhenNotMatchedBySourceActionType.Update)
        {
            builder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} {tableAlias}".TrimEnd());
            builder.AppendLine($"SET {action.SetClause}");
        }

        builder.AppendLine($"WHERE {whereClause}");

        if (!string.IsNullOrEmpty(action.AndCondition))
        {
            builder.AppendLine($"AND {action.AndCondition}");
        }

        builder.Append(";");
        return builder.ToString();
    }

    public BulkMergeResult Execute(IReadOnlyCollection<T> data)
    {
        if (data.Count == 1)
        {
            return SingleMerge(data.First());
        }

        var whenNotMatchedBySourceAction = GetWhenNotMatchedBySourceAction("t", "s");
        bool hasWhenNotMatchedBySourceAction = HasWhenNotMatchedBySourceAction(whenNotMatchedBySourceAction);

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any() && !hasWhenNotMatchedBySourceAction)
        {
            return new BulkMergeResult();
        }

        var temptableName = $"`{Guid.NewGuid()}`";

        var propertyNames = _updateColumnNames.ToList();
        propertyNames.AddRange(_mergeKeys);
        propertyNames.AddRange(_insertColumnNames);
        propertyNames = propertyNames.Distinct().ToList();

        var dataTable = data.ToDataTable(propertyNames, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _table.ColumnTypeMappings);
        sqlCreateTemptable += $"\n{CreateIndex(temptableName)}";

        var joinCondition = CreateJoinCondition(dataTable);

        var whereCondition = CreateWhereIsNullCondition();

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();
        string notMatchedBySourceStatement = null;

        if (_updateColumnNames.Any())
        {
            updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} t JOIN {temptableName} s ON " + joinCondition);
            updateStatementBuilder.AppendLine($"SET {string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => CreateSetStatement(x, "t", "s")))};");
        }

        if (_insertColumnNames.Any())
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName}({_table.CreateDbColumnNames(_insertColumnNames, includeDiscriminator: true)})");
            insertStatementBuilder.AppendLine($"SELECT {_table.CreateColumnNames(_insertColumnNames, "s", includeDiscriminator: true)}");
            insertStatementBuilder.AppendLine($"FROM {temptableName} s");
            insertStatementBuilder.AppendLine($"LEFT JOIN {_table.SchemaQualifiedTableName} t ON {joinCondition}");
            insertStatementBuilder.AppendLine($"WHERE {whereCondition};");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var notExistsWhereClause = $"NOT EXISTS (SELECT 1 FROM {temptableName} s WHERE {joinCondition})";
            notMatchedBySourceStatement = BuildNotMatchedBySourceStatement(whenNotMatchedBySourceAction.Value, "t", notExistsWhereClause);
        }

        _connectionContext.EnsureOpen();

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        using (var createTemptableCommand = _connectionContext.CreateTextCommand(sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        _connectionContext.SqlBulkCopy(dataTable, temptableName, null, _options);
        Log("End executing SqlBulkCopy.");

        var result = new BulkMergeResult();
        var notMatchedBySourceRows = 0;

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connectionContext.CreateTextCommand(sqlUpdateStatement, _options);

            result.UpdatedRows = updateCommand.ExecuteNonQuery();

            Log("End updating.");
        }

        if (_insertColumnNames.Any())
        {
            var sqlInsertStatement = insertStatementBuilder.ToString();

            Log($"Begin inserting:{Environment.NewLine}{sqlInsertStatement}");

            using var insertCommand = _connectionContext.CreateTextCommand(sqlInsertStatement, _options);

            result.InsertedRows = insertCommand.ExecuteNonQuery();

            Log("End inserting.");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            Log($"Begin when not matched by source:{Environment.NewLine}{notMatchedBySourceStatement}");

            using var notMatchedBySourceCommand = _connectionContext.CreateTextCommand(notMatchedBySourceStatement, _options);

            var actionParameters = whenNotMatchedBySourceAction?.Parameters.ToMySqlParameterInfors() ?? [];
            foreach (var param in actionParameters)
            {
                notMatchedBySourceCommand.Parameters.Add(param.Parameter);
            }
            LogParameters(actionParameters);

            notMatchedBySourceRows = notMatchedBySourceCommand.ExecuteNonQuery();

            Log("End when not matched by source.");
        }

        result.AffectedRows = result.UpdatedRows + result.InsertedRows + notMatchedBySourceRows;
        return result;
    }

    private string CreateSetStatement(string prop, string leftTable, string rightTable)
    {
        return _table.CreateSetClause(prop, leftTable, rightTable, _options.ConfigureSetClause);
    }

    private string CreateSetStatement(string prop)
    {
        return _table.CreateSetClause(prop, _options.ConfigureSetClause);
    }

    private string CreateWhereStatement(string prop)
    {
        string sqlOperator = "=";

        return $"`{_table.GetDbColumnName(prop)}` {sqlOperator} {_table.CreateParameterName(prop)}";
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMerge]: {message}");
    }

    private void LogParameters(List<ParameterInfo> parameters)
    {
        if (_options?.LogTo == null)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            _options.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMerge][Parameter]: {parameter}");
        }
    }

    public async Task<BulkMergeResult> ExecuteAsync(IReadOnlyCollection<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count == 1)
        {
            return await SingleMergeAsync(data.First(), cancellationToken);
        }

        var whenNotMatchedBySourceAction = GetWhenNotMatchedBySourceAction("t", "s");
        bool hasWhenNotMatchedBySourceAction = HasWhenNotMatchedBySourceAction(whenNotMatchedBySourceAction);

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any() && !hasWhenNotMatchedBySourceAction)
        {
            return new BulkMergeResult();
        }

        var temptableName = $"`{Guid.NewGuid()}`";

        var propertyNames = _updateColumnNames.ToList();
        propertyNames.AddRange(_mergeKeys);
        propertyNames.AddRange(_insertColumnNames);
        propertyNames = propertyNames.Distinct().ToList();

        var dataTable = await data.ToDataTableAsync(propertyNames, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _table.ColumnTypeMappings);
        sqlCreateTemptable += $"\n{CreateIndex(temptableName)}";

        var joinCondition = CreateJoinCondition(dataTable);

        var whereCondition = CreateWhereIsNullCondition();

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();
        string notMatchedBySourceStatement = null;

        if (_updateColumnNames.Any())
        {
            updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} t JOIN {temptableName} s ON " + joinCondition);
            updateStatementBuilder.AppendLine($"SET {string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => CreateSetStatement(x, "t", "s")))};");
        }

        if (_insertColumnNames.Any())
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName}({_table.CreateDbColumnNames(_insertColumnNames, includeDiscriminator: true)})");
            insertStatementBuilder.AppendLine($"SELECT {_table.CreateColumnNames(_insertColumnNames, "s", includeDiscriminator: true)}");
            insertStatementBuilder.AppendLine($"FROM {temptableName} s");
            insertStatementBuilder.AppendLine($"LEFT JOIN {_table.SchemaQualifiedTableName} t ON {joinCondition}");
            insertStatementBuilder.AppendLine($"WHERE {whereCondition};");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var notExistsWhereClause = $"NOT EXISTS (SELECT 1 FROM {temptableName} s WHERE {joinCondition})";
            notMatchedBySourceStatement = BuildNotMatchedBySourceStatement(whenNotMatchedBySourceAction.Value, "t", notExistsWhereClause);
        }

        await _connectionContext.EnsureOpenAsync(cancellationToken);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        using (var createTemptableCommand = _connectionContext.CreateTextCommand(sqlCreateTemptable, _options))
        {
            await createTemptableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        await _connectionContext.SqlBulkCopyAsync(dataTable, temptableName, null, _options, cancellationToken);
        Log("End executing SqlBulkCopy.");

        var result = new BulkMergeResult();
        var notMatchedBySourceRows = 0;

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connectionContext.CreateTextCommand(sqlUpdateStatement, _options);

            result.UpdatedRows = await updateCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End updating.");
        }

        if (_insertColumnNames.Any())
        {
            var sqlInsertStatement = insertStatementBuilder.ToString();

            Log($"Begin inserting:{Environment.NewLine}{sqlInsertStatement}");

            using var insertCommand = _connectionContext.CreateTextCommand(sqlInsertStatement, _options);

            result.InsertedRows = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End inserting.");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            Log($"Begin when not matched by source:{Environment.NewLine}{notMatchedBySourceStatement}");

            using var notMatchedBySourceCommand = _connectionContext.CreateTextCommand(notMatchedBySourceStatement, _options);

            var actionParameters = whenNotMatchedBySourceAction?.Parameters.ToMySqlParameterInfors() ?? [];
            foreach (var param in actionParameters)
            {
                notMatchedBySourceCommand.Parameters.Add(param.Parameter);
            }
            LogParameters(actionParameters);

            notMatchedBySourceRows = await notMatchedBySourceCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End when not matched by source.");
        }

        result.AffectedRows = result.UpdatedRows + result.InsertedRows + notMatchedBySourceRows;
        return result;
    }

    public BulkMergeResult SingleMerge(T data)
    {
        var whenNotMatchedBySourceAction = GetWhenNotMatchedBySourceAction("", "");
        bool hasWhenNotMatchedBySourceAction = HasWhenNotMatchedBySourceAction(whenNotMatchedBySourceAction);

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any() && !hasWhenNotMatchedBySourceAction)
        {
            return new BulkMergeResult();
        }

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();
        string notMatchedBySourceStatement = null;

        if (_updateColumnNames.Any())
        {
            var whereCondition = CreateWhereCondition();

            updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} SET");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => CreateSetStatement(x))));
            updateStatementBuilder.AppendLine($"WHERE {whereCondition}");
        }

        if (_insertColumnNames.Any())
        {
            var whereCondition = CreateWhereNotExistsCondition();

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName}({_table.CreateDbColumnNames(_insertColumnNames, includeDiscriminator: true)})");
            insertStatementBuilder.AppendLine($"SELECT {_table.CreateParameterNames(_insertColumnNames, includeDiscriminator: true)}");
            insertStatementBuilder.AppendLine($"WHERE NOT EXISTS ({whereCondition});");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var whereNotExistsCondition = CreateWhereCondition();
            var notWhereClause = $"NOT ({whereNotExistsCondition})";
            notMatchedBySourceStatement = BuildNotMatchedBySourceStatement(whenNotMatchedBySourceAction.Value, "", notWhereClause);
        }

        _connectionContext.EnsureOpen();

        var result = new BulkMergeResult();
        var notMatchedBySourceRows = 0;

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            var propertyNamesIncludeId = _updateColumnNames.ToList();
            propertyNamesIncludeId.AddRange(_mergeKeys);
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connectionContext.CreateTextCommand(sqlUpdateStatement, _options);
            LogParameters(_table.CreateMySqlParameters(updateCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            result.UpdatedRows = updateCommand.ExecuteNonQuery();

            Log("End updating.");
        }

        if (_insertColumnNames.Any() && result.UpdatedRows == 0)
        {
            var sqlInsertStatement = insertStatementBuilder.ToString();

            var propertyNamesIncludeId = _insertColumnNames.ToList();
            propertyNamesIncludeId.AddRange(_mergeKeys);
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin inserting:{Environment.NewLine}{sqlInsertStatement}");

            using var insertCommand = _connectionContext.CreateTextCommand(sqlInsertStatement, _options);
            LogParameters(_table.CreateMySqlParameters(insertCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            result.InsertedRows = insertCommand.ExecuteNonQuery();

            Log("End inserting.");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var propertyNamesIncludeId = _mergeKeys.ToList();
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin when not matched by source:{Environment.NewLine}{notMatchedBySourceStatement}");

            using var notMatchedBySourceCommand = _connectionContext.CreateTextCommand(notMatchedBySourceStatement, _options);
            LogParameters(_table.CreateMySqlParameters(notMatchedBySourceCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            var actionParameters = whenNotMatchedBySourceAction?.Parameters.ToMySqlParameterInfors() ?? [];
            foreach (var param in actionParameters)
            {
                notMatchedBySourceCommand.Parameters.Add(param.Parameter);
            }
            LogParameters(actionParameters);

            notMatchedBySourceRows = notMatchedBySourceCommand.ExecuteNonQuery();

            Log("End when not matched by source.");
        }

        result.AffectedRows = result.UpdatedRows + result.InsertedRows + notMatchedBySourceRows;
        return result;
    }

    public async Task<BulkMergeResult> SingleMergeAsync(T data, CancellationToken cancellationToken = default)
    {
        var whenNotMatchedBySourceAction = GetWhenNotMatchedBySourceAction("", "");
        bool hasWhenNotMatchedBySourceAction = HasWhenNotMatchedBySourceAction(whenNotMatchedBySourceAction);

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any() && !hasWhenNotMatchedBySourceAction)
        {
            return new BulkMergeResult();
        }

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();
        string notMatchedBySourceStatement = null;

        if (_updateColumnNames.Any())
        {
            var whereCondition = CreateWhereCondition();

            updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} SET");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => CreateSetStatement(x))));
            updateStatementBuilder.AppendLine($"WHERE {whereCondition}");
        }

        if (_insertColumnNames.Any())
        {
            var whereCondition = CreateWhereNotExistsCondition();

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName}({_table.CreateDbColumnNames(_insertColumnNames, includeDiscriminator: true)})");
            insertStatementBuilder.AppendLine($"SELECT {_table.CreateParameterNames(_insertColumnNames, includeDiscriminator: true)}");
            insertStatementBuilder.AppendLine($"WHERE NOT EXISTS ({whereCondition});");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var whereNotExistsCondition = CreateWhereCondition();
            var notWhereClause = $"NOT ({whereNotExistsCondition})";
            notMatchedBySourceStatement = BuildNotMatchedBySourceStatement(whenNotMatchedBySourceAction.Value, "", notWhereClause);
        }

        await _connectionContext.EnsureOpenAsync(cancellationToken: cancellationToken);

        var result = new BulkMergeResult();
        var notMatchedBySourceRows = 0;

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            var propertyNamesIncludeId = _updateColumnNames.ToList();
            propertyNamesIncludeId.AddRange(_mergeKeys);
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connectionContext.CreateTextCommand(sqlUpdateStatement, _options);
            LogParameters(_table.CreateMySqlParameters(updateCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            result.UpdatedRows = await updateCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End updating.");
        }

        if (_insertColumnNames.Any() && result.UpdatedRows == 0)
        {
            var sqlInsertStatement = insertStatementBuilder.ToString();

            var propertyNamesIncludeId = _insertColumnNames.ToList();
            propertyNamesIncludeId.AddRange(_mergeKeys);
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin inserting:{Environment.NewLine}{sqlInsertStatement}");

            using var insertCommand = _connectionContext.CreateTextCommand(sqlInsertStatement, _options);
            LogParameters(_table.CreateMySqlParameters(insertCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            result.InsertedRows = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End inserting.");
        }

        if (hasWhenNotMatchedBySourceAction)
        {
            var propertyNamesIncludeId = _mergeKeys.ToList();
            propertyNamesIncludeId = propertyNamesIncludeId.Distinct().ToList();

            Log($"Begin when not matched by source:{Environment.NewLine}{notMatchedBySourceStatement}");

            using var notMatchedBySourceCommand = _connectionContext.CreateTextCommand(notMatchedBySourceStatement, _options);
            LogParameters(_table.CreateMySqlParameters(notMatchedBySourceCommand, data, propertyNamesIncludeId, includeDiscriminator: true, autoAdd: true));

            var actionParameters = whenNotMatchedBySourceAction?.Parameters.ToMySqlParameterInfors() ?? [];
            foreach (var param in actionParameters)
            {
                notMatchedBySourceCommand.Parameters.Add(param.Parameter);
            }
            LogParameters(actionParameters);

            notMatchedBySourceRows = await notMatchedBySourceCommand.ExecuteNonQueryAsync(cancellationToken);

            Log("End when not matched by source.");
        }

        result.AffectedRows = result.UpdatedRows + result.InsertedRows + notMatchedBySourceRows;
        return result;
    }
}
