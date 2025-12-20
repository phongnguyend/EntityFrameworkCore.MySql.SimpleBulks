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
            return CreateSetStatement(x);
        }));
    }

    private string CreateWhereNotExistsCondition()
    {
        return $"SELECT 1 FROM {_table.SchemaQualifiedTableName} WHERE " + string.Join(" AND ", GetKeys().Select(x =>
        {
            return CreateSetStatement(x);
        }));
    }

    private string CreateIndex(string tableName)
    {
        return $"CREATE INDEX Idx_Id ON {tableName} ({string.Join(",", GetKeys().Select(x => $"`{x}`"))});";
    }

    public BulkMergeResult Execute(IReadOnlyCollection<T> data)
    {
        if (data.Count == 1)
        {
            return SingleMerge(data.First());
        }

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
        {
            return new BulkMergeResult();
        }

        var temptableName = $"`{Guid.NewGuid()}`";

        var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
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

        result.AffectedRows = result.UpdatedRows + result.InsertedRows;
        return result;
    }

    private string CreateSetStatement(string prop, string leftTable, string rightTable)
    {
        string sqlOperator = "=";
        string sqlProp = RemoveOperator(prop);

        if (prop.EndsWith("+="))
        {
            sqlOperator = "+=";
        }

        return $"{leftTable}.`{_table.GetDbColumnName(sqlProp)}` {sqlOperator} {rightTable}.`{sqlProp}`";
    }

    private string CreateSetStatement(string prop)
    {
        string sqlOperator = "=";
        string sqlProp = RemoveOperator(prop);

        if (prop.EndsWith("+="))
        {
            sqlOperator = "+=";
        }

        return $"`{_table.GetDbColumnName(sqlProp)}` {sqlOperator} {_table.CreateParameterName(sqlProp)}";
    }

    private static string RemoveOperator(string prop)
    {
        var rs = prop.Replace("+=", "");
        return rs;
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

        if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
        {
            return new BulkMergeResult();
        }

        var temptableName = $"`{Guid.NewGuid()}`";

        var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
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

        result.AffectedRows = result.UpdatedRows + result.InsertedRows;
        return result;
    }

    public BulkMergeResult SingleMerge(T data)
    {
        if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
        {
            return new BulkMergeResult();
        }

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();

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

        _connectionContext.EnsureOpen();

        var result = new BulkMergeResult();

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            var propertyNamesIncludeId = _updateColumnNames.Select(RemoveOperator).ToList();
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

        result.AffectedRows = result.UpdatedRows + result.InsertedRows;
        return result;
    }

    public async Task<BulkMergeResult> SingleMergeAsync(T data, CancellationToken cancellationToken = default)
    {
        if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
        {
            return new BulkMergeResult();
        }

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();

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

        await _connectionContext.EnsureOpenAsync(cancellationToken: cancellationToken);

        var result = new BulkMergeResult();

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            var propertyNamesIncludeId = _updateColumnNames.Select(RemoveOperator).ToList();
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

        result.AffectedRows = result.UpdatedRows + result.InsertedRows;
        return result;
    }
}
