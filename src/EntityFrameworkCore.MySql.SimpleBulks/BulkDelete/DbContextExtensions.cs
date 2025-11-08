using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkDeleteBuilder<T>()
            .WithId(table.PrimaryKeys)
            .ToTable(table)
    .WithBulkOptions(options)
      .Execute(data);
    }
}
