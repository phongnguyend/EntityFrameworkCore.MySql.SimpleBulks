using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("MySqlCollection")]
public class BulkInsertTests : BaseTest
{
    public BulkInsertTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkInsert")
    {
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1, false)]
    [InlineData(100, true)]
    [InlineData(100, false)]
    public void BulkInsert_Without_Transaction(int length, bool useLinq)
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        var bulkId = SequentialGuidGenerator.Next();

        for (int i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn,
                ComplexShippingAddress = new ComplexTypeAddress
                {
                    Street = "Street " + i,
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                OwnedShippingAddress = new OwnedTypeAddress
                {
                    Street = "Street " + i,
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                BulkId = bulkId,
                BulkIndex = i
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn,
            });
        }

        var connectionContext = new ConnectionContext(_connection, null);

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            connectionContext.BulkInsert(rows,
                row => new
                {
                    row.Column1,
                    row.Column2,
                    row.Column3,
                    row.Season,
                    row.SeasonAsString,
                    row.NullableBool,
                    row.NullableDateTime,
                    row.NullableDateTimeOffset,
                    row.NullableDecimal,
                    row.NullableDouble,
                    row.NullableGuid,
                    row.NullableShort,
                    row.NullableInt,
                    row.NullableLong,
                    row.NullableFloat,
                    row.NullableString,
                    row.ComplexShippingAddress.Street,
                    row.ComplexShippingAddress.Location.Lat,
                    row.ComplexShippingAddress.Location.Lng,
                    a = row.OwnedShippingAddress.Street,
                    b = row.OwnedShippingAddress.Location.Lat,
                    c = row.OwnedShippingAddress.Location.Lng,
                    row.BulkId,
                    row.BulkIndex
                },
                options: options);

            connectionContext.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                options: options);
        }
        else
        {
            connectionContext.BulkInsert(rows,
                [
                    "Column1",
                    "Column2",
                    "Column3",
                    "ComplexShippingAddress.Street",
                    "ComplexShippingAddress.Location.Lat",
                    "ComplexShippingAddress.Location.Lng",
                    "OwnedShippingAddress.Street",
                    "OwnedShippingAddress.Location.Lat",
                    "OwnedShippingAddress.Location.Lng",
                    "BulkId",
                    "BulkIndex"
                ],
                options: options);

            connectionContext.BulkInsert(compositeKeyRows,
                ["Id1", "Id2", "Column1", "Column2", "Column3"],
                options: options);
        }

        var ids = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).ToDictionary(x => x.BulkIndex!.Value, x => x.Id);

        foreach (var row in rows)
        {
            row.Id = ids[row.BulkIndex!.Value];
        }


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

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
    public void BulkInsert_KeepIdentity(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var options = new BulkInsertOptions
        {
            KeepIdentity = true,
            LogTo = LogTo
        };

        connectionContext.BulkInsert(configurationEntries, options: options);

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BulkInsert_Return_GeneratedId(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

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

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.BulkInsert(configurationEntries, options: options);

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (int i = 0; i < length; i++)
        {
            Assert.NotEqual(Guid.Empty, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime.TruncateToMicroseconds(), configurationEntriesInDb[i].CreatedDateTime);
        }
    }
}