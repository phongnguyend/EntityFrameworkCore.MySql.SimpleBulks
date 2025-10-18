using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, Action<BulkDeleteOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();

        return new BulkDeleteBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .SingleDelete(data);
    }
}
