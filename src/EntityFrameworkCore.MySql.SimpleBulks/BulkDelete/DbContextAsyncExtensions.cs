﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkDeleteBuilder<T>(connectionContext)
      .WithId(dbContext.GetPrimaryKeys(typeof(T)))
        .ToTable(dbContext.GetTableInfor(typeof(T)))
     .WithBulkOptions(options)
     .ExecuteAsync(data, cancellationToken);
    }
}
