using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;

public static class ConnectionContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, MySqlTableInfor<T> table = null, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(temp.PropertyNames)
            .WithTable(temp)
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, MySqlTableInfor<T> table = null, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
   .WithReturnedColumns(returnedColumnsSelector)
  .WithTable(table ?? TableMapper.Resolve<T>())
 .WithMatchedColumns(matchedColumnsSelector)
      .WithBulkOptions(options)
      .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, IReadOnlyCollection<string> matchedColumns, IReadOnlyCollection<string> returnedColumns, MySqlTableInfor<T> table = null, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(returnedColumns)
       .WithTable(table ?? TableMapper.Resolve<T>())
     .WithMatchedColumns(matchedColumns)
     .WithBulkOptions(options)
       .ExecuteAsync(machedValues, cancellationToken);
    }
}