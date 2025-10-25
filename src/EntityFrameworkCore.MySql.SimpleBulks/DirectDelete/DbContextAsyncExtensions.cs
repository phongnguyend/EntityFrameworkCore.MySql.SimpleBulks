using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();

        return new BulkDeleteBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .ConfigureBulkOptions(configureOptions)
             .SingleDeleteAsync(data, cancellationToken);
    }
}
