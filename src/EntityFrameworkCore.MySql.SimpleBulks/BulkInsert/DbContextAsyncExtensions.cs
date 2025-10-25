﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

public static class DbContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connection, transaction)
              .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
              .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
              .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
              .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
              .ToTable(dbContext.GetTableInfor(typeof(T)))
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .ConfigureBulkOptions(configureOptions)
              .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connection, transaction)
              .WithColumns(columnNamesSelector)
              .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
              .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
              .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
              .ToTable(dbContext.GetTableInfor(typeof(T)))
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .ConfigureBulkOptions(configureOptions)
              .ExecuteAsync(data, cancellationToken);
    }

    private static OutputIdMode GetOutputIdMode(ColumnInfor columnInfor)
    {
        if (columnInfor == null)
        {
            return OutputIdMode.ServerGenerated;
        }

        return columnInfor.PropertyType == typeof(Guid) && string.IsNullOrEmpty(columnInfor.DefaultValueSql) ? OutputIdMode.ClientGenerated : OutputIdMode.ServerGenerated;
    }
}
