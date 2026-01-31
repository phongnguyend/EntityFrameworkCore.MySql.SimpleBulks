using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.MySql.SimpleBulks.DirectInsert;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("MySqlCollection")]
public class DirectInsertTests : BaseTest
{
    public DirectInsertTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.DirectInsert")
    {
    }

    [Fact]
    public void DirectInsert_Using_Linq_Without_Transaction()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var bulkId = SequentialGuidGenerator.Next();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring,
            BulkId = bulkId,
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString, row.BulkId },
                options: options);

        row.Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        connectionContext.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options: options);


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3.TruncateToMicroseconds(), dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void DirectInsert_Using_Linq_With_Transaction_Committed()
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var bulkId = SequentialGuidGenerator.Next();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring,
            BulkId = bulkId,
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString, row.BulkId },
                options: options);

        connectionContext.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options: options);

        tran.Commit();

        row.Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3.TruncateToMicroseconds(), dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void DirectInsert_Using_Linq_With_Transaction_RolledBack()
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options: options);

        connectionContext.DirectInsert(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options: options);

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Fact]
    public void DirectInsert_KeepIdentity()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntry = new ConfigurationEntry
        {
            Id = Guid.NewGuid(),
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        var options = new BulkInsertOptions
        {
            KeepIdentity = true,
            LogTo = LogTo
        };

        connectionContext.DirectInsert(configurationEntry, options: options);

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[0].CreatedDateTime);
    }

    [Fact]
    public void DirectInsert_Return_GeneratedId()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntry = new ConfigurationEntry
        {
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(configurationEntry, options: options);

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.NotEqual(Guid.Empty, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[0].CreatedDateTime);
    }

    [Fact]
    public void DirectInsert_Using_DynamicString()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var bulkId = SequentialGuidGenerator.Next();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring,
            BulkId = bulkId,
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
            [
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString",
            "BulkId"
            ],
            options: options);

        row.Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        connectionContext.DirectInsert(compositeKeyRow,
            [
            "Id1",
            "Id2",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
            ],
            options: options);

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3.TruncateToMicroseconds(), dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }
}