using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkDeleteOptions> configureOptions = null)
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
             .Execute(data);
    }
}
