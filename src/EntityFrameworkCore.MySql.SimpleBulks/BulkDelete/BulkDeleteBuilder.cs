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
    private BulkDeleteOptions _options;
    private readonly MySqlConnection _connection;
    private readonly MySqlTransaction _transaction;

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
        if (_table.ColumnNameMappings == null)
        {
            return columnName;
        }

        return _table.ColumnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    public BulkDeleteResult Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            return SingleDelete(data.First());
        }

        var temptableName = $"`{Guid.NewGuid()}`";
        var dataTable = data.ToDataTable(_idColumns, valueConverters: _table.ValueConverters);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _table.ColumnTypeMappings);
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

        _table.CreateMySqlParameters(deleteCommand, dataToDelete, _idColumns).ForEach(x => deleteCommand.Parameters.Add(x));

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

    public async Task<BulkDeleteResult> ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count() == 1)
        {
            return await SingleDeleteAsync(data.First(), cancellationToken);
        }

        var temptableName = $"`{Guid.NewGuid()}`";
        var dataTable = await data.ToDataTableAsync(_idColumns, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTempTableDefinition(temptableName, null, _table.ColumnTypeMappings);
        sqlCreateTemptable += $"\nCREATE UNIQUE INDEX Idx_Id ON {temptableName} ({string.Join(",", _idColumns.Select(x => $"`{x}`"))});";

        var joinCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.`{GetDbColumnName(x)}`{collation} = b.`{x}`{collation}";
        }));

        var deleteStatement = $"DELETE a FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition;

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        await _connection.EnsureOpenAsync(cancellationToken);

        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            await createTemptableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        Log("End creating temp table.");


        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

        await dataTable.SqlBulkCopyAsync(temptableName, null, _connection, _transaction, _options, cancellationToken);

        Log("End executing SqlBulkCopy.");

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        var affectedRows = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    public async Task<BulkDeleteResult> SingleDeleteAsync(T dataToDelete, CancellationToken cancellationToken = default)
    {
        var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            return $"`{GetDbColumnName(x)}` = @{x}";
        }));

        var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        _table.CreateMySqlParameters(deleteCommand, dataToDelete, _idColumns).ForEach(x => deleteCommand.Parameters.Add(x));

        await _connection.EnsureOpenAsync(cancellationToken);

        var affectedRows = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }
}
