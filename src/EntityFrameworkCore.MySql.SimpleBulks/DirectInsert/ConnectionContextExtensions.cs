using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectInsert;

public static class ConnectionContextExtensions
{
    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, MySqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
    .SingleInsert(data);
    }

    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, MySqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNames)
             .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
           .SingleInsert(data);
    }
}