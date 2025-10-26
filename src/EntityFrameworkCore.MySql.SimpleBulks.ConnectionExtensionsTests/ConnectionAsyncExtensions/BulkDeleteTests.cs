using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.ConnectionAsyncExtensions;

[Collection("MySqlCollection")]
public class BulkDeleteTests : BaseTest
{
    public BulkDeleteTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkDelete")
    {
        TableMapper.Register(typeof(SingleKeyRow<int>), GetTableName("SingleKeyRows"));
        TableMapper.Register(typeof(CompositeKeyRow<int, int>), GetTableName("CompositeKeyRows"));

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < 100; i++)
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
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        if (useLinq)
        {
            if (omitTableName)
            {
                await _connection.BulkDeleteAsync(rows, row => row.Id,
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
                await _connection.BulkDeleteAsync(compositeKeyRows, row => new { row.Id1, row.Id2 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            }
            else
            {
                await _connection.BulkDeleteAsync(rows, new TableInfor(GetTableName("SingleKeyRows")), row => row.Id,
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
                await _connection.BulkDeleteAsync(compositeKeyRows, new TableInfor(GetTableName("CompositeKeyRows")), row => new { row.Id1, row.Id2 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            }
        }
        else
        {
            if (omitTableName)
            {
                await _connection.BulkDeleteAsync(rows, "Id",
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
                await _connection.BulkDeleteAsync(compositeKeyRows, ["Id1", "Id2"],
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            }
            else
            {
                await _connection.BulkDeleteAsync(rows, new TableInfor(GetTableName("SingleKeyRows")), "Id",
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
                await _connection.BulkDeleteAsync(compositeKeyRows, new TableInfor(GetTableName("CompositeKeyRows")), ["Id1", "Id2"],
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }
}