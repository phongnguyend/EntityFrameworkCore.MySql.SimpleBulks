using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

public class BulkInsertBuilder<T>
{
    private TableInfor _table;
    private string _outputIdColumn;
    private OutputIdMode _outputIdMode = OutputIdMode.ServerGenerated;
    private IEnumerable<string> _columnNames;
    private BulkInsertOptions _options;
    private readonly ConnectionContext _connectionContext;

    public BulkInsertBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkInsertBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkInsertBuilder<T> WithOutputId(string idColumn)
    {
        _outputIdColumn = idColumn;
        return this;
    }

    public BulkInsertBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
    {
        _outputIdColumn = idSelector.Body.GetMemberName();
        return this;
    }

    public BulkInsertBuilder<T> WithOutputIdMode(OutputIdMode outputIdMode)
    {
        _outputIdMode = outputIdMode;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkInsertBuilder<T> ConfigureBulkOptions(Action<BulkInsertOptions> configureOptions)
    {
        _options = new BulkInsertOptions();
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

    private PropertyInfo GetIdProperty()
    {
        return typeof(T).GetProperty(_outputIdColumn);
    }

    private static Action<T, Guid> GetSetIdMethod(PropertyInfo idProperty)
    {
        return (Action<T, Guid>)Delegate.CreateDelegate(typeof(Action<T, Guid>), idProperty.GetSetMethod());
    }

    public void Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            SingleInsert(data.First());
            return;
        }

        DataTable dataTable;
        if (string.IsNullOrWhiteSpace(_outputIdColumn))
        {
            dataTable = data.ToDataTable(_columnNames, valueConverters: _table.ValueConverters);

            _connectionContext.Connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            dataTable = data.ToDataTable(columnsToInsert, valueConverters: _table.ValueConverters);

            _connectionContext.Connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            dataTable = data.ToDataTable(columnsToInsert, valueConverters: _table.ValueConverters);

            _connectionContext.Connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        dataTable = data.ToDataTable(_columnNames, valueConverters: _table.ValueConverters);

        _connectionContext.Connection.EnsureOpen();

        Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
        dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options);
        Log("End executing SqlBulkCopy.");
        return;
    }

    public void SingleInsert(T dataToInsert)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }
        }
        else if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);

            setId(dataToInsert, SequentialGuidGenerator.Next());
        }

        insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"`{GetDbColumnName(x)}`"))})");
        insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connectionContext.Connection.CreateTextCommand(_connectionContext.Transaction, insertStatement, _options);
        _table.CreateMySqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        _connectionContext.Connection.EnsureOpen();

        var affectedRow = insertCommand.ExecuteNonQuery();

        Log($"End inserting.");
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert]: {message}");
    }

    public async Task ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count() == 1)
        {
            await SingleInsertAsync(data.First(), cancellationToken);
            return;
        }

        DataTable dataTable;
        if (string.IsNullOrWhiteSpace(_outputIdColumn))
        {
            dataTable = await data.ToDataTableAsync(_columnNames, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.Connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            dataTable = await data.ToDataTableAsync(columnsToInsert, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.Connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            dataTable = await data.ToDataTableAsync(columnsToInsert, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.Connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        dataTable = await data.ToDataTableAsync(_columnNames, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

        await _connectionContext.Connection.EnsureOpenAsync(cancellationToken);

        Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
        await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _table.ColumnNameMappings, _connectionContext.Connection, _connectionContext.Transaction, _options, cancellationToken);
        Log("End executing SqlBulkCopy.");
        return;
    }

    public async Task SingleInsertAsync(T dataToInsert, CancellationToken cancellationToken = default)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }
        }
        else if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);

            setId(dataToInsert, SequentialGuidGenerator.Next());
        }

        insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"`{GetDbColumnName(x)}`"))})");
        insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connectionContext.Connection.CreateTextCommand(_connectionContext.Transaction, insertStatement, _options);
        _table.CreateMySqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        await _connectionContext.Connection.EnsureOpenAsync(cancellationToken);

        var affectedRow = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

        Log($"End inserting.");
    }
}
