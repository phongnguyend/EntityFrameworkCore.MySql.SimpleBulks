using MySqlConnector;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class MySqlConnectionExtensions
{
    public static void EnsureOpen(this MySqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    public static async Task EnsureOpenAsync(this MySqlConnection connection, CancellationToken cancellationToken = default)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    public static void EnsureClosed(this MySqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public static MySqlCommand CreateTextCommand(this MySqlConnection connection, MySqlTransaction transaction, string commandText, BulkOptions options = null)
    {
        options ??= new BulkOptions()
        {
            BatchSize = 0,
            Timeout = 30,
        };

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.CommandTimeout = options.Timeout;
        return command;
    }
}
