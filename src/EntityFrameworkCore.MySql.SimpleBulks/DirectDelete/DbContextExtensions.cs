﻿using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EntityFrameworkCore.MySql.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, Action<BulkDeleteOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var primaryKeys = properties
            .Where(x => x.IsPrimaryKey)
            .Select(x => x.PropertyName);
        var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

        return new BulkDeleteBuilder<T>(connection, transaction)
             .WithId(primaryKeys)
             .WithDbColumnMappings(dbColumnMappings)
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .SingleDelete(data);
    }
}
