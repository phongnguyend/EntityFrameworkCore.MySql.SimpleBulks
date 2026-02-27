using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.Upsert;

public static class DbContextAsyncExtensions
{
    public static Task<BulkMergeResult> UpsertAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return dbContext.CreateBulkMergeBuilder<T>()
      .WithId(idSelector)
 .WithUpdateColumns(updateColumnNamesSelector)
   .WithInsertColumns(insertColumnNamesSelector)
    .ToTable(dbContext.GetTableInfor<T>())
      .WithBulkOptions(options)
      .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        if (options?.ConfigureWhenNotMatchedBySource != null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return dbContext.CreateBulkMergeBuilder<T>()
      .WithId(idColumns)
       .WithUpdateColumns(updateColumnNames)
      .WithInsertColumns(insertColumnNames)
        .ToTable(dbContext.GetTableInfor<T>())
      .WithBulkOptions(options)
          .SingleMergeAsync(data, cancellationToken);
    }
}
