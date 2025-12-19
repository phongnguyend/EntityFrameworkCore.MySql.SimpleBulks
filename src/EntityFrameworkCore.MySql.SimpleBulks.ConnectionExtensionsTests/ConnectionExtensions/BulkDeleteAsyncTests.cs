using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("MySqlCollection")]
public class BulkDeleteAsyncTests : BaseTest
{
    public BulkDeleteAsyncTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkDelete")
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < 100; i++)
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

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);
        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        var options = new BulkDeleteOptions
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, options: options);
            }
            else
            {
                await connectionContext.BulkDeleteAsync(rows, new MySqlTableInfor<SingleKeyRow<int>>(GetTableName("SingleKeyRows"))
                {
                    PrimaryKeys = ["Id"],
                }, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, new MySqlTableInfor<CompositeKeyRow<int, int>>(GetTableName("CompositeKeyRows"))
                {
                    PrimaryKeys = ["Id1", "Id2"],
                }, options: options);
            }
        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, options: options);
            }
            else
            {
                await connectionContext.BulkDeleteAsync(rows, new MySqlTableInfor<SingleKeyRow<int>>(GetTableName("SingleKeyRows"))
                {
                    PrimaryKeys = ["Id"],
                }, options: options);
                await connectionContext.BulkDeleteAsync(compositeKeyRows, new MySqlTableInfor<CompositeKeyRow<int, int>>(GetTableName("CompositeKeyRows"))
                {
                    PrimaryKeys = ["Id1", "Id2"],
                }, options: options);
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }
}