using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public class BulkDeleteBuilder<T>
{
    private TableInfor _table;
    private IEnumerable<string> _idColumns;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private BulkDeleteOptions _options;
    private readonly MySqlConnection _connection;
    private readonly MySqlTransaction _transaction;

    public BulkDeleteBuilder(MySqlConnection connection)
    {
        _connection = connection;
    }

    public BulkDeleteBuilder(MySqlConnection connection, MySqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkDeleteBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(string idColumn)
    {
        _idColumns = [idColumn];
        return this;
    }

    public BulkDeleteBuilder<T> WithId(IEnumerable<string> idColumns)
    {
        _idColumns = idColumns;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
        return this;
    }

    public BulkDeleteBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> dbColumnMappings)
    {
        _columnNameMappings = dbColumnMappings;
        return this;
    }

    public BulkDeleteBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public BulkDeleteBuilder<T> ConfigureBulkOptions(Action<BulkDeleteOptions> configureOptions)
    {
        _options = new BulkDeleteOptions();
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

    public BulkDeleteResult Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            return SingleDelete(data.First());
        }

        var temptableName = $"`{Guid.NewGuid()}`";
        var dataTable = data.ToDataTable(_idColumns);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _columnTypeMappings);
        sqlCreateTemptable += $"\nCREATE UNIQUE INDEX Idx_Id ON {temptableName} ({string.Join(",", _idColumns.Select(x => $"`{x}`"))});";

        var joinCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.`{GetDbColumnName(x)}`{collation} = b.`{x}`{collation}";
        }));

        var deleteStatement = $"DELETE a FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition;

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        _connection.EnsureOpen();

        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");


        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

        dataTable.SqlBulkCopy(temptableName, null, _connection, _transaction, _options);

        Log("End executing SqlBulkCopy.");

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        var affectedRows = deleteCommand.ExecuteNonQuery();

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    public BulkDeleteResult SingleDelete(T dataToDelete)
    {
        var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            return $"`{GetDbColumnName(x)}` = @{x}";
        }));

        var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        dataToDelete.ToMySqlParameters(_idColumns).ForEach(x => deleteCommand.Parameters.Add(x));

        _connection.EnsureOpen();

        var affectedRows = deleteCommand.ExecuteNonQuery();

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkDelete]: {message}");
    }

    public Task<BulkDeleteResult> ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute(data));
    }

    public Task<BulkDeleteResult> SingleDeleteAsync(T dataToDelete, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(SingleDelete(dataToDelete));
    }
}
