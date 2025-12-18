using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("MySqlCollection")]
public class BulkUpdateAsyncTests : BaseTest
{
    public BulkUpdateAsyncTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkUpdate")
    {
    }

    private void SeedData(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Summer,
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
                }
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i + 1,
                Id2 = i + 1,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Summer
            });
        }

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Update_Using_Linq_With_Transaction(int length)
    {
        SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Autumn;
            row.SeasonAsString = Season.Winter;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Autumn;
            row.SeasonAsString = Season.Winter;
        }

        var updateResult1 = await _context.BulkUpdateAsync(rows,
    row => new
    {
        row.Column3,
        row.Column2,
        row.Season,
        row.SeasonAsString,
        row.ComplexShippingAddress.Street,
        row.ComplexShippingAddress.Location.Lat,
        row.ComplexShippingAddress.Location.Lng,
        a = row.OwnedShippingAddress.Street,
        b = row.OwnedShippingAddress.Location.Lat,
        c = row.OwnedShippingAddress.Location.Lng
    },
       new BulkUpdateOptions
       {
           LogTo = LogTo
       });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
   row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
     new BulkUpdateOptions
     {
         LogTo = LogTo
     });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
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
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Update_Using_Dynamic_String_With_Transaction(int length)
    {
        SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Autumn;
            row.SeasonAsString = Season.Winter;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Autumn;
            row.SeasonAsString = Season.Winter;
        }

        var updateResult1 = await _context.BulkUpdateAsync(rows,
              ["Column3", "Column2", "Season", "SeasonAsString"],
        new BulkUpdateOptions
        {
            LogTo = LogTo
        });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
    ["Column3", "Column2", "Season", "SeasonAsString"],
      new BulkUpdateOptions
      {
          LogTo = LogTo
      });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
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
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }
}