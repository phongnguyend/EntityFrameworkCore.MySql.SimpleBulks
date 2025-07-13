using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.MySqlConnectionExtensions;

[Collection("MySqlCollection")]
public class BulkInsertTests : BaseTest
{
    private string _schema = "";

    public BulkInsertTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkInsert")
    {
        TableMapper.Register(typeof(SingleKeyRow<int>), _schema, "SingleKeyRows");
        TableMapper.Register(typeof(CompositeKeyRow<int, int>), _schema, "CompositeKeyRows");
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Bulk_Insert_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        var bulkId = SequentialGuidGenerator.Next();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                BulkId = bulkId,
                BulkIndex = i
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

        if (useLinq)
        {
            if (omitTableName)
            {
                _connection.BulkInsert(rows,
                    row => new { row.Column1, row.Column2, row.Column3, row.BulkId, row.BulkIndex },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkInsert(rows, new TableInfor(_schema, "SingleKeyRows"),
                    row => new { row.Column1, row.Column2, row.Column3, row.BulkId, row.BulkIndex },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
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
                _connection.BulkInsert(rows,
                    ["Column1", "Column2", "Column3", "BulkId", "BulkIndex"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows,
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkInsert(rows, new TableInfor(_schema, "SingleKeyRows"),
                    ["Column1", "Column2", "Column3", "BulkId", "BulkIndex"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkInsert(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
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

        for (int i = 0; i < 100; i++)
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