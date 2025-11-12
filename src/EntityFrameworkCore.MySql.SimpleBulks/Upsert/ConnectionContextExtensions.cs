using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Upsert;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, MySqlTableInfor<T> table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
   .WithId(idSelector)
      .WithUpdateColumns(updateColumnNamesSelector)
    .WithInsertColumns(insertColumnNamesSelector)
 .ToTable(table ?? TableMapper.Resolve<T>())
    .WithBulkOptions(options)
    .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, MySqlTableInfor<T> table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
          .WithId(idColumns)
      .WithUpdateColumns(updateColumnNames)
      .WithInsertColumns(insertColumnNames)
        .ToTable(table ?? TableMapper.Resolve<T>())
 .WithBulkOptions(options)
        .SingleMerge(data);
    }
}