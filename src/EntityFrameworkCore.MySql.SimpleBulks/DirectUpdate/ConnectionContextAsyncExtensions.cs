using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, MySqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
    .WithId(temp.PrimaryKeys)
.WithColumns(columnNamesSelector)
    .ToTable(temp)
 .WithBulkOptions(options)
   .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, MySqlTableInfor<T> table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
     .WithId(temp.PrimaryKeys)
    .WithColumns(columnNames)
 .ToTable(temp)
     .WithBulkOptions(options)
        .SingleUpdateAsync(data, cancellationToken);
    }
}