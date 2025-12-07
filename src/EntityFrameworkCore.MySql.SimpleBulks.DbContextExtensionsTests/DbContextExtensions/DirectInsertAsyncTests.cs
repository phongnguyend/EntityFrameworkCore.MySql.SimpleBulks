using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.MySql.SimpleBulks.DirectInsert;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("MySqlCollection")]
public class DirectInsertAsyncTests : BaseTest
{
    public DirectInsertAsyncTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.DirectInsert")
    {
    }

    [Fact]
    public async Task Direct_Insert_Using_Linq_Without_Transaction()
    {
        var bulkId = SequentialGuidGenerator.Next();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street " + 1,
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street " + 1,
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            },
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
            SeasonAsString = Season.Summer
        };

        var options = new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        };

        await _context.DirectInsertAsync(row,
                row => new
                {
                    row.Column1,
                    row.Column2,
                    row.Column3,
                    row.Season,
                    row.SeasonAsString,
                    row.ComplexShippingAddress.Street,
                    row.ComplexShippingAddress.Location.Lat,
                    row.ComplexShippingAddress.Location.Lng,
                    a = row.OwnedShippingAddress.Street,
                    b = row.OwnedShippingAddress.Location.Lat,
                    c = row.OwnedShippingAddress.Location.Lng,
                    row.BulkId
                },
                options);

        row.Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        await _context.DirectInsertAsync(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options);


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
        Assert.Equal(row.ComplexShippingAddress?.Street, dbRows[0].ComplexShippingAddress?.Street);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lat, dbRows[0].ComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lng, dbRows[0].ComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.OwnedShippingAddress?.Street, dbRows[0].OwnedShippingAddress?.Street);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lat, dbRows[0].OwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lng, dbRows[0].OwnedShippingAddress?.Location?.Lng);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public async Task Direct_Insert_Using_Linq_With_Transaction_Committed()
    {
        var tran = _context.Database.BeginTransaction();

        var bulkId = SequentialGuidGenerator.Next();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street " + 1,
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street " + 1,
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            },
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
            SeasonAsString = Season.Summer
        };

        var options = new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        };

        await _context.DirectInsertAsync(row,
                row => new
                {
                    row.Column1,
                    row.Column2,
                    row.Column3,
                    row.Season,
                    row.SeasonAsString,
                    row.ComplexShippingAddress.Street,
                    row.ComplexShippingAddress.Location.Lat,
                    row.ComplexShippingAddress.Location.Lng,
                    a = row.OwnedShippingAddress.Street,
                    b = row.OwnedShippingAddress.Location.Lat,
                    c = row.OwnedShippingAddress.Location.Lng,
                    row.BulkId
                },
                options);

        row.Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        await _context.DirectInsertAsync(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options);

        tran.Commit();

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
        Assert.Equal(row.ComplexShippingAddress?.Street, dbRows[0].ComplexShippingAddress?.Street);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lat, dbRows[0].ComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lng, dbRows[0].ComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.OwnedShippingAddress?.Street, dbRows[0].OwnedShippingAddress?.Street);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lat, dbRows[0].OwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lng, dbRows[0].OwnedShippingAddress?.Location?.Lng);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3.TruncateToMicroseconds(), dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public async Task Direct_Insert_Using_Linq_With_Transaction_RolledBack()
    {
        var tran = _context.Database.BeginTransaction();

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street " + 1,
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street " + 1,
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128 + 1,
                    Lng = -74.0060 - 1
                }
            }
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Summer
        };

        var options = new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        };

        await _context.DirectInsertAsync(row,
                row => new
                {
                    row.Column1,
                    row.Column2,
                    row.Column3,
                    row.Season,
                    row.SeasonAsString,
                    row.ComplexShippingAddress.Street,
                    row.ComplexShippingAddress.Location.Lat,
                    row.ComplexShippingAddress.Location.Lng,
                    a = row.OwnedShippingAddress.Street,
                    b = row.OwnedShippingAddress.Location.Lat,
                    c = row.OwnedShippingAddress.Location.Lng
                },
                options);

        await _context.DirectInsertAsync(compositeKeyRow,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options);

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Fact]
    public async Task Direct_Insert_KeepIdentity()
    {
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
            LogTo = _output.WriteLine
        };

        await _context.DirectInsertAsync(configurationEntry, options);

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
    public async Task Direct_Insert_Return_GeneratedId()
    {
        var configurationEntry = new ConfigurationEntry
        {
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        var options = new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        };

        await _context.DirectInsertAsync(configurationEntry, options);

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
}