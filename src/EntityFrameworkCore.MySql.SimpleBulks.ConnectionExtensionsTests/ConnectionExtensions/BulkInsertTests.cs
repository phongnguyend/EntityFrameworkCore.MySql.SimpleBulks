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
    [InlineData(1, true, true)]
    [InlineData(1, true, false)]
    [InlineData(1, false, true)]
    [InlineData(1, false, false)]
    [InlineData(100, true, true)]
    [InlineData(100, true, false)]
    [InlineData(100, false, true)]
    [InlineData(100, false, false)]
    public void Bulk_Insert_Without_Transaction(int length, bool useLinq, bool omitTableName)
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
            });
        }

        var connectionContext = new ConnectionContext(_connection, null);

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                connectionContext.BulkInsert(rows,
                    row => new
                    {
                        row.Column1,
                        row.Column2,
                        row.Column3,
                        row.Season,
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
                    row => new
                    {
                        row.Column1,
                        row.Column2,
                        row.Column3,
                        row.Season,
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
                        row.BulkId,
                        row.BulkIndex
                    },
                    new MySqlTableInfor<SingleKeyRow<int>>(GetTableName("SingleKeyRows")),
                    options: options);

                connectionContext.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    new MySqlTableInfor<CompositeKeyRow<int, int>>(GetTableName("CompositeKeyRows")),
                    options: options);
            }

        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkInsert(rows,
                    ["Column1", "Column2", "Column3", "BulkId", "BulkIndex"],
                    options: options);

                connectionContext.BulkInsert(compositeKeyRows,
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options: options);
            }
            else
            {
                connectionContext.BulkInsert(rows,
                    ["Column1", "Column2", "Column3", "BulkId", "BulkIndex"],
                    new MySqlTableInfor<SingleKeyRow<int>>(GetTableName("SingleKeyRows")),
                    options: options);

                connectionContext.BulkInsert(compositeKeyRows,
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    new MySqlTableInfor<CompositeKeyRow<int, int>>(GetTableName("CompositeKeyRows")),
                    options: options);
            }

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

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3.TruncateToMicroseconds(), dbCompositeKeyRows[i].Column3);
        }
    }
}