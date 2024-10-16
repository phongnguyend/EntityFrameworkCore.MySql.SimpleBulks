﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public class BulkMergeBuilder<T>
{
    private TableInfor _table;
    private IEnumerable<string> _idColumns;
    private IEnumerable<string> _updateColumnNames;
    private IEnumerable<string> _insertColumnNames;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private string _outputIdColumn;
    private BulkMergeOptions _options;
    private readonly MySqlConnection _connection;
    private readonly MySqlTransaction _transaction;

    public BulkMergeBuilder(MySqlConnection connection)
    {
        _connection = connection;
    }

    public BulkMergeBuilder(MySqlConnection connection, MySqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkMergeBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkMergeBuilder<T> WithId(string idColumn)
    {
        _idColumns = [idColumn];
        return this;
    }

    public BulkMergeBuilder<T> WithId(IEnumerable<string> idColumns)
    {
        _idColumns = idColumns;
        return this;
    }

    public BulkMergeBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : [idColumn];
        return this;
    }

    public BulkMergeBuilder<T> WithUpdateColumns(IEnumerable<string> updateColumnNames)
    {
        _updateColumnNames = updateColumnNames;
        return this;
    }

    public BulkMergeBuilder<T> WithUpdateColumns(Expression<Func<T, object>> updateColumnNamesSelector)
    {
        _updateColumnNames = updateColumnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkMergeBuilder<T> WithInsertColumns(IEnumerable<string> insertColumnNames)
    {
        _insertColumnNames = insertColumnNames;
        return this;
    }

    public BulkMergeBuilder<T> WithInsertColumns(Expression<Func<T, object>> insertColumnNamesSelector)
    {
        _insertColumnNames = insertColumnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkMergeBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public BulkMergeBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public BulkMergeBuilder<T> WithOutputId(string idColumn)
    {
        _outputIdColumn = idColumn;
        return this;
    }

    public BulkMergeBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
    {
        _outputIdColumn = idSelector.Body.GetMemberName();
        return this;
    }

    public BulkMergeBuilder<T> ConfigureBulkOptions(Action<BulkMergeOptions> configureOptions)
    {
        _options = new BulkMergeOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetDbColumnName(string columnName)
    {
        if (_columnNameMappings == null)
        {
            return columnName;
        }

        return _columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    public BulkMergeResult Execute(IEnumerable<T> data)
    {
        if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
        {
            return new BulkMergeResult();
        }

        var temptableName = $"`{Guid.NewGuid()}`";

        var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
        propertyNames.AddRange(_idColumns);
        propertyNames.AddRange(_insertColumnNames);
        propertyNames = propertyNames.Distinct().ToList();

        var dataTable = data.ToDataTable(propertyNames);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _columnTypeMappings);
        sqlCreateTemptable += $"\nCREATE INDEX Idx_Id ON {temptableName} ({string.Join(",", _idColumns.Select(x => $"`{x}`"))});";

        var joinCondition = string.Join(" and ", _idColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" collate {_options.Collation}" : string.Empty;
            return $"s.`{x}`{collation} = t.`{GetDbColumnName(x)}`{collation}";
        }));

        var whereCondition = string.Join(" and ", _idColumns.Select(x =>
        {
            return $"t.`{GetDbColumnName(x)}` IS NULL";
        }));

        var insertStatementBuilder = new StringBuilder();
        var updateStatementBuilder = new StringBuilder();

        if (_updateColumnNames.Any())
        {
            updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} t JOIN {temptableName} s ON " + joinCondition);
            updateStatementBuilder.AppendLine($"SET {string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => CreateSetStatement(x, "t", "s")))};");
        }

        if (_insertColumnNames.Any())
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName}({string.Join(", ", _insertColumnNames.Select(x => $"`{GetDbColumnName(x)}`"))})");
            insertStatementBuilder.AppendLine($"SELECT {string.Join(", ", _insertColumnNames.Select(x => $"s.`{x}`"))}");
            insertStatementBuilder.AppendLine($"FROM {temptableName} s");
            insertStatementBuilder.AppendLine($"LEFT JOIN {_table.SchemaQualifiedTableName} t ON {joinCondition}");
            insertStatementBuilder.AppendLine($"WHERE {whereCondition};");
        }

        _connection.EnsureOpen();

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        dataTable.SqlBulkCopy(temptableName, null, _connection, _transaction, _options);
        Log("End executing SqlBulkCopy.");

        var result = new BulkMergeResult();

        if (_updateColumnNames.Any())
        {
            var sqlUpdateStatement = updateStatementBuilder.ToString();

            Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

            using var updateCommand = _connection.CreateTextCommand(_transaction, sqlUpdateStatement, _options);

            result.UpdatedRows = updateCommand.ExecuteNonQuery();

            Log("End updating.");
        }

        if (_insertColumnNames.Any())
        {
            var sqlInsertStatement = insertStatementBuilder.ToString();

            Log($"Begin inserting:{Environment.NewLine}{sqlInsertStatement}");

            using var insertCommand = _connection.CreateTextCommand(_transaction, sqlInsertStatement, _options);

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

        return $"{leftTable}.`{GetDbColumnName(sqlProp)}` {sqlOperator} {rightTable}.`{sqlProp}`";
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
}
