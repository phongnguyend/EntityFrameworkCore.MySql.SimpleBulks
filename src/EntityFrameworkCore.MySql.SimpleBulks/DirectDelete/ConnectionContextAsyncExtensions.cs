using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, MySqlTableInfor table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
        .WithId(idSelector)
         .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
         .SingleDeleteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, MySqlTableInfor table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
           .WithId(idColumns)
       .ToTable(table ?? TableMapper.Resolve<T>())
         .WithBulkOptions(options)
          .SingleDeleteAsync(data, cancellationToken);
    }
}