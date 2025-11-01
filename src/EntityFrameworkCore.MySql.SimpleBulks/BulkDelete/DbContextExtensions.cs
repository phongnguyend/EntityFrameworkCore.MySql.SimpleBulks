﻿using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null)
    {
        return new BulkDeleteBuilder<T>(dbContext.GetConnectionContext())
      .WithId(dbContext.GetPrimaryKeys(typeof(T)))
   .ToTable(dbContext.GetTableInfor(typeof(T)))
  .WithBulkOptions(options)
         .Execute(data);
    }
}
