using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;

public static class ConnectionContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, MySqlTableInfor table = null, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
   .WithReturnedColumns(returnedColumnsSelector)
  .WithTable(table ?? TableMapper.Resolve<T>())
 .WithMatchedColumns(matchedColumnsSelector)
      .WithBulkOptions(options)
      .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, MySqlTableInfor table = null, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(returnedColumns)
       .WithTable(table ?? TableMapper.Resolve<T>())
     .WithMatchedColumns(matchedColumns)
     .WithBulkOptions(options)
       .ExecuteAsync(machedValues, cancellationToken);
    }
}