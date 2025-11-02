using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

public static class ConnectionContextExtensions
{
    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNamesSelector)
       .ToTable(TableMapper.Resolve<T>())
       .WithBulkOptions(options)
    .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNames)
             .ToTable(TableMapper.Resolve<T>())
          .WithBulkOptions(options)
           .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
      .WithColumns(columnNamesSelector)
     .ToTable(table)
       .WithBulkOptions(options)
     .Execute(data);
    }

    public static void BulkInsert<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
              .WithColumns(columnNames)
                      .ToTable(table)
       .WithBulkOptions(options)
         .Execute(data);
    }
}