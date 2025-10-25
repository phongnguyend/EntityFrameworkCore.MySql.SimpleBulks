﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;

public static class DbContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connection, transaction)
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
            .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
            .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
            .WithOutputId(outputIdColumn)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connection, transaction)
            .WithId(idColumn)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
            .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
            .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
            .WithOutputId(outputIdColumn)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connection, transaction)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
            .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
            .WithValueConverters(dbContext.GetValueConverters(typeof(T)))
            .WithOutputId(outputIdColumn)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }
}
