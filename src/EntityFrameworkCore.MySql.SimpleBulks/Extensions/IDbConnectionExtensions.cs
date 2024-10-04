﻿using MySqlConnector;
using System.Data;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class IDbConnectionExtensions
{
    public static MySqlConnection AsMySqlConnection(this IDbConnection connection)
    {
        return connection as MySqlConnection;
    }

    public static void EnsureOpen(this IDbConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    public static void EnsureClosed(this IDbConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public static IDbCommand CreateTextCommand(this IDbConnection connection, IDbTransaction transaction, string commandText, BulkOptions options = null)
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
