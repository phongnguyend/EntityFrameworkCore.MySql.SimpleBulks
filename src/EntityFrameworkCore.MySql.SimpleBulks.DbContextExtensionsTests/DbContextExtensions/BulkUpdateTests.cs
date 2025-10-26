using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("MySqlCollection")]
public class BulkUpdateTests : BaseTest
{
    public BulkUpdateTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkUpdate")
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
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Summer
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

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        tran.Commit();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Update_Using_Linq_With_Transaction(int length)
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

        var updateResult1 = _context.BulkUpdate(rows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);

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
    public void Bulk_Update_Using_Dynamic_String_With_Transaction(int length)
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

        var updateResult1 = _context.BulkUpdate(rows,
              ["Column3", "Column2", "Season", "SeasonAsString"],
              options =>
              {
                  options.LogTo = _output.WriteLine;
              });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3.TruncateToMicroseconds(), dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);

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