using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

public static class MySqlConnectionAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext.Connection)
             .WithColumns(columnNamesSelector)
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext.Connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext.Connection)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext.Connection)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }
}
