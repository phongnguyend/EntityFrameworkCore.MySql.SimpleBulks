using EntityFrameworkCore.MySql.SimpleBulks;
using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.Demo;
using EntityFrameworkCore.MySql.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using (var dbct = new DemoDbContext())
{
    dbct.Database.Migrate();

    var deleteResult = dbct.BulkDelete(dbct.Set<ConfigurationEntry>().AsNoTracking(),
          opt =>
          {
              opt.LogTo = Console.WriteLine;
          });

    Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");

    var configurationEntries = new List<ConfigurationEntry>();

    for (int i = 0; i < 1000; i++)
    {
        configurationEntries.Add(new ConfigurationEntry
        {
            Key = $"Key{i}",
            Value = $"Value{i}",
            CreatedDateTime = DateTimeOffset.Now,
        });
    }

    dbct.BulkInsert(configurationEntries,
        opt =>
        {
            opt.LogTo = Console.WriteLine;
        });

    foreach (var row in configurationEntries)
    {
        row.Key += "xx";
        row.UpdatedDateTime = DateTimeOffset.Now;
        row.IsSensitive = true;
        row.Description = row.Id.ToString();
    }

    var updateResult = dbct.BulkUpdate(configurationEntries,
        x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description },
        opt =>
        {
            opt.LogTo = Console.WriteLine;
        });

    Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");

    configurationEntries.Add(new ConfigurationEntry
    {
        Id = SequentialGuidGenerator.Next(),
        Key = $"Key{1001}",
        Value = $"Value{1001}",
        CreatedDateTime = DateTimeOffset.Now,
    });

    var mergeResult = dbct.BulkMerge(configurationEntries,
        x => x.Id,
        x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description },
        x => new { x.Id, x.Key, x.Value, x.IsSensitive, x.CreatedDateTime },
        opt =>
        {
            opt.LogTo = Console.WriteLine;
        });

    Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
    Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
    Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");
}

Console.WriteLine("Finished!");
Console.ReadLine();