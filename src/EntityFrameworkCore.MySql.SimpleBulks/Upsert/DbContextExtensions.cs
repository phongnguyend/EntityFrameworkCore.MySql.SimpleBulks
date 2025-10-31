using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Upsert;

public static class DbContextExtensions
{
    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connectionContext)
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .WithOutputId(outputIdColumn)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithBulkOptions(options)
            .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connectionContext)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .WithOutputId(outputIdColumn)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithBulkOptions(options)
            .SingleMerge(data);
    }
}
