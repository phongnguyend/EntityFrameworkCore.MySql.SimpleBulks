using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectUpdate;

public static class DbContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();

        return new BulkUpdateBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this DbContext dbContext, T data, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();

        return new BulkUpdateBuilder<T>(connection, transaction)
            .WithId(dbContext.GetPrimaryKeys(typeof(T)))
            .WithColumns(columnNames)
            .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
            .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleUpdateAsync(data, cancellationToken);
    }
}
