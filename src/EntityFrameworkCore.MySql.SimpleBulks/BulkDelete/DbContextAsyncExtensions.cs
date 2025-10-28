using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();

        return new BulkDeleteBuilder<T>(connection, transaction)
            .WithId(dbContext.GetPrimaryKeys(typeof(T)))
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }
}
