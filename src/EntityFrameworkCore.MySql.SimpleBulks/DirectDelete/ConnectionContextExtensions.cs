using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, MySqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
         .WithId(idSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
       .WithBulkOptions(options)
            .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, MySqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
    .WithId(idColumns)
   .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
           .SingleDelete(data);
    }
}