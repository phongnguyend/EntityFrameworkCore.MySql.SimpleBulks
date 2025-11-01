using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.TempTable;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class ConnectionContextExtensions
{
    private static readonly BulkOptions DefaultBulkOptions = new BulkOptions()
    {
        BatchSize = 0,
        Timeout = 30,
    };

    public static void EnsureOpen(this ConnectionContext connection)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Connection.Open();
        }
    }

    public static async Task EnsureOpenAsync(this ConnectionContext connection, CancellationToken cancellationToken = default)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Open)
        {
            await connection.Connection.OpenAsync(cancellationToken);
        }
    }

    public static void EnsureClosed(this ConnectionContext connection)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Connection.Close();
        }
    }

    public static MySqlCommand CreateTextCommand(this ConnectionContext connection, string commandText, BulkOptions options = null)
    {
        options ??= DefaultBulkOptions;

        var command = connection.Connection.CreateCommand();
        command.Transaction = connection.Transaction;
        command.CommandText = commandText;
        command.CommandTimeout = options.Timeout;
        return command;
    }

    public static void SqlBulkCopy(this ConnectionContext connectionContext, DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, BulkOptions options = null)
    {
        options ??= DefaultBulkOptions;

        var bulkCopy = new MySqlBulkCopy(connectionContext.Connection, connectionContext.Transaction)
        {
            BulkCopyTimeout = options.Timeout,
            DestinationTableName = $"{tableName}"
        };

        int idx = 0;

        foreach (DataColumn dtColum in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(idx, GetDbColumnName(dtColum.ColumnName, columnNameMappings)));
            idx++;
        }

        bulkCopy.WriteToServer(dataTable);
    }

    public static async Task SqlBulkCopyAsync(this ConnectionContext connectionContext, DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, BulkOptions options = null, CancellationToken cancellationToken = default)
    {
        options ??= DefaultBulkOptions;

        var bulkCopy = new MySqlBulkCopy(connectionContext.Connection, connectionContext.Transaction)
        {
            BulkCopyTimeout = options.Timeout,
            DestinationTableName = $"{tableName}"
        };

        int idx = 0;

        foreach (DataColumn dtColum in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(idx, GetDbColumnName(dtColum.ColumnName, columnNameMappings)));
            idx++;
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }

    private static string GetDbColumnName(string columnName, IReadOnlyDictionary<string, string> columnNameMappings)
    {
        if (columnNameMappings == null)
        {
            return columnName;
        }

        return columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    public static BulkInsertBuilder<T> CreateBulkInsertBuilder<T>(this ConnectionContext connectionContext)
    {
        return new BulkInsertBuilder<T>(connectionContext);
    }

    public static BulkUpdateBuilder<T> CreateBulkUpdateBuilder<T>(this ConnectionContext connectionContext)
    {
        return new BulkUpdateBuilder<T>(connectionContext);
    }

    public static BulkDeleteBuilder<T> CreateBulkDeleteBuilder<T>(this ConnectionContext connectionContext)
    {
        return new BulkDeleteBuilder<T>(connectionContext);
    }

    public static BulkMergeBuilder<T> CreateBulkMergeBuilder<T>(this ConnectionContext connectionContext)
    {
        return new BulkMergeBuilder<T>(connectionContext);
    }

    public static BulkMatchBuilder<T> CreateBulkMatchBuilder<T>(this ConnectionContext connectionContext)
    {
        return new BulkMatchBuilder<T>(connectionContext);
    }

    public static TempTableBuilder<T> CreateTempTableBuilder<T>(this ConnectionContext connectionContext)
    {
        return new TempTableBuilder<T>(connectionContext);
    }
}
