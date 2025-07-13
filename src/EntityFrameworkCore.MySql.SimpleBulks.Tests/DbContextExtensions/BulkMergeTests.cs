﻿using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

[Collection("MySqlCollection")]
public class BulkMergeTests : BaseTest
{
    public BulkMergeTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkMerge")
    {
    }

    private void SeedData(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now
            });
        }

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3 });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

        tran.Commit();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkMerge_Using_Linq_With_Transaction(int length)
    {
        SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var bulkId = SequentialGuidGenerator.Next();

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length,
            Column2 = "Inserted using Merge" + length,
            Column3 = DateTime.Now,
            BulkId = bulkId
        });

        var newId1 = length;
        var newId2 = length;

        compositeKeyRows.Add(new CompositeKeyRow<int, int>
        {
            Id1 = newId1,
            Id2 = newId2,
            Column1 = newId2,
            Column2 = "Inserted using Merge" + newId2,
            Column3 = DateTime.Now,
        });

        _context.BulkMerge(rows,
                row => row.Id,
                row => new { row.Column1, row.Column2, row.Column3 },
                row => new { row.Column1, row.Column2, row.Column3, row.BulkId },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        rows.First(x => x.BulkId == bulkId).Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        _context.BulkMerge(compositeKeyRows,
                row => new { row.Id1, row.Id2 },
                row => new { row.Column1, row.Column2, row.Column3 },
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkMerge_Using_Dynamic_String_With_Transaction(int length)
    {
        SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var bulkId = SequentialGuidGenerator.Next();

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length,
            Column2 = "Inserted using Merge" + length,
            Column3 = DateTime.Now,
            BulkId = bulkId
        });

        var newId1 = length;
        var newId2 = length;

        compositeKeyRows.Add(new CompositeKeyRow<int, int>
        {
            Id1 = newId1,
            Id2 = newId2,
            Column1 = newId2,
            Column2 = "Inserted using Merge" + newId2,
            Column3 = DateTime.Now,
        });

        _context.BulkMerge(rows,
            "Id",
            ["Column1", "Column2", "Column3"],
            ["Column1", "Column2", "Column3", "BulkId"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        rows.First(x => x.BulkId == bulkId).Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        _context.BulkMerge(compositeKeyRows,
            ["Id1", "Id2"],
            ["Column1", "Column2", "Column3"],
            ["Id1", "Id2", "Column1", "Column2", "Column3"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkMerge_UpdateOnly(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries, options =>
        {
            options.LogTo = _output.WriteLine;
        });

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (int i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = _context.BulkMerge(configurationEntries,
             x => x.Id,
             x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
             x => new { },
             options =>
             {
                 options.LogTo = _output.WriteLine;
             });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(length, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkMerge_InsertOnly(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries, options =>
        {
            options.LogTo = _output.WriteLine;
        });

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (int i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Id = SequentialGuidGenerator.Next(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = _context.BulkMerge(configurationEntries,
             x => x.Id,
             x => new { },
             x => new { x.Id, x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
             options =>
             {
                 options.LogTo = _output.WriteLine;
             });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length, result.AffectedRows);
        Assert.Equal(length, result.InsertedRows);
        Assert.Equal(0, result.UpdatedRows);
        Assert.Equal(length * 2, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.NotEqual(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
            Assert.NotEqual(configurationEntries[i].UpdatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].UpdatedDateTime);
        }

        for (int i = length; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkMerge_DoNothing(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries, options =>
        {
            options.LogTo = _output.WriteLine;
        });

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (int i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = _context.BulkMerge(configurationEntries,
             x => x.Id,
             x => new { },
             x => new { },
             options =>
             {
                 options.LogTo = _output.WriteLine;
             });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(0, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(0, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.NotEqual(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
            Assert.NotEqual(configurationEntries[i].UpdatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].UpdatedDateTime);
        }
    }
}