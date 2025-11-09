using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, MySqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .Execute(data);
    }
}