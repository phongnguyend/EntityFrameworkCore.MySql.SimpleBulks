using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, MySqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNamesSelector)
    .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
.SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> columnNames, MySqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
        .WithColumns(columnNames)
       .ToTable(table ?? TableMapper.Resolve<T>())
           .WithBulkOptions(options)
         .SingleInsertAsync(data, cancellationToken);
    }
}