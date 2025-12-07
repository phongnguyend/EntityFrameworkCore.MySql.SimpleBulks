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
    private TableInfor<T> _table;
    private string _outputIdColumn;
    private OutputIdMode _outputIdMode = OutputIdMode.ServerGenerated;
    private IReadOnlyCollection<string> _columnNames;
    private BulkInsertOptions _options = BulkInsertOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkInsertBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkInsertBuilder<T> ToTable(TableInfor<T> table)
    {
        _table = table;
        _outputIdColumn = table?.OutputId?.Name;
        _outputIdMode = table?.OutputId?.Mode ?? OutputIdMode.ServerGenerated;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(IReadOnlyCollection<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkInsertBuilder<T> WithBulkOptions(BulkInsertOptions options)
    {
        _options = options ?? BulkInsertOptions.DefaultOptions;
        return this;
    }

    private PropertyInfo GetIdProperty()
    {
        return PropertiesCache<T>.GetProperty(_outputIdColumn);
    }

    private static Action<T, Guid> GetSetIdMethod(PropertyInfo idProperty)
    {
        return (Action<T, Guid>)Delegate.CreateDelegate(typeof(Action<T, Guid>), idProperty.GetSetMethod());
    }

    public void Execute(IReadOnlyCollection<T> data)
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

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
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

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
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

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        dataTable = data.ToDataTable(_columnNames, valueConverters: _table.ValueConverters);

        _connectionContext.EnsureOpen();

        Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
        _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
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

        insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"`{_table.GetDbColumnName(x)}`"))})");
        insertStatementBuilder.AppendLine($"VALUES ({_table.CreateParameterNames(columnsToInsert)})");

        var insertStatement = insertStatementBuilder.ToString();

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        using var insertCommand = _connectionContext.CreateTextCommand(insertStatement, _options);
        LogParameters(_table.CreateMySqlParameters(insertCommand, dataToInsert, columnsToInsert, autoAdd: true));

        _connectionContext.EnsureOpen();

        var affectedRow = insertCommand.ExecuteNonQuery();

        Log($"End inserting.");
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert]: {message}");
    }

    private void LogParameters(List<ParameterInfo> parameters)
    {
        if (_options?.LogTo == null)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            _options.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert][Parameter]: {parameter}");
        }
    }

    public async Task ExecuteAsync(IReadOnlyCollection<T> data, CancellationToken cancellationToken = default)
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

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
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

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
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

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        dataTable = await data.ToDataTableAsync(_columnNames, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

        await _connectionContext.EnsureOpenAsync(cancellationToken);

        Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
        await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
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

        insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"`{_table.GetDbColumnName(x)}`"))})");
        insertStatementBuilder.AppendLine($"VALUES ({_table.CreateParameterNames(columnsToInsert)})");

        var insertStatement = insertStatementBuilder.ToString();

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        using var insertCommand = _connectionContext.CreateTextCommand(insertStatement, _options);
        LogParameters(_table.CreateMySqlParameters(insertCommand, dataToInsert, columnsToInsert, autoAdd: true));

        await _connectionContext.EnsureOpenAsync(cancellationToken);

        var affectedRow = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

        Log($"End inserting.");
    }
}
