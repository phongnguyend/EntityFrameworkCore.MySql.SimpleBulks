using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, MySqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
  .WithId(temp.PrimaryKeys)
   .WithColumns(columnNamesSelector)
      .ToTable(temp)
  .WithBulkOptions(options)
  .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, MySqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
       .WithId(temp.PrimaryKeys)
            .WithColumns(columnNames)
       .ToTable(temp)
           .WithBulkOptions(options)
          .Execute(data);
    }
}