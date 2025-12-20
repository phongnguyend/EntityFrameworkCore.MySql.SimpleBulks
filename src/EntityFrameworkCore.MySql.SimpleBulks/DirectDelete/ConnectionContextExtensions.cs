using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, MySqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> keySelector, MySqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> keys, MySqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }
}