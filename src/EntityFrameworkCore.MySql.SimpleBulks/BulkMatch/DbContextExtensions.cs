﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkMatch;

public static class DbContextExtensions
{
    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var columns = properties.Select(x => x.PropertyName);
        var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(columns)
             .WithDbColumnMappings(dbColumnMappings)
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetMySqlConnection();
        var transaction = dbContext.GetCurrentMySqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(returnedColumnsSelector)
             .WithDbColumnMappings(dbColumnMappings)
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }
}
