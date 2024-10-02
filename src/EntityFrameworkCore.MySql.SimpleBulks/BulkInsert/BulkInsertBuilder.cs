using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

public class BulkInsertBuilder<T>
{
    private TableInfor _table;
    private string _outputIdColumn;
    private OutputIdMode _outputIdMode = OutputIdMode.ServerGenerated;
    private IEnumerable<string> _columnNames;
    private IDictionary<string, string> _dbColumnMappings;
    private BulkInsertOptions _options;
    private readonly MySqlConnection _connection;
    private readonly MySqlTransaction _transaction;

    public BulkInsertBuilder(MySqlConnection connection)
    {
        _connection = connection;
    }

    public BulkInsertBuilder(MySqlConnection connection, MySqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
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

    public BulkInsertBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
    {
        _dbColumnMappings = dbColumnMappings;
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
        if (_dbColumnMappings == null)
        {
            return columnName;
        }

        return _dbColumnMappings.TryGetValue(columnName, out string value) ? value : columnName;
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
            dataTable = data.ToDataTable(_columnNames);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _dbColumnMappings, _connection, _transaction, _options);
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

            dataTable = data.ToDataTable(columnsToInsert);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _dbColumnMappings, _connection, _transaction, _options);
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

            var idProperty = typeof(T).GetProperty(_outputIdColumn);

            foreach (var row in data)
            {
                idProperty.SetValue(row, SequentialGuidGenerator.Next());
            }

            dataTable = data.ToDataTable(columnsToInsert);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _dbColumnMappings, _connection, _transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        dataTable = data.ToDataTable(_columnNames);

        _connection.EnsureOpen();

        Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
        dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _dbColumnMappings, _connection, _transaction, _options);
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

            var idProperty = typeof(T).GetProperty(_outputIdColumn);

            idProperty.SetValue(dataToInsert, SequentialGuidGenerator.Next());
        }

        insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"`{GetDbColumnName(x)}`"))})");
        insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);
        dataToInsert.ToMySqlParameters(columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        _connection.EnsureOpen();

        var affectedRow = insertCommand.ExecuteNonQuery();

        Log($"End inserting.");
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert]: {message}");
    }
}
