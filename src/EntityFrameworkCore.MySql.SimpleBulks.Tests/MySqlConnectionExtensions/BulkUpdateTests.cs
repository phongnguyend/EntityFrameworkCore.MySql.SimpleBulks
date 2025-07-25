﻿using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.MySqlConnectionExtensions;

[Collection("MySqlCollection")]
public class BulkUpdateTests : BaseTest
{
    private string _schema = "";

    public BulkUpdateTests(ITestOutputHelper output, MySqlFixture fixture) : base(output, fixture, "SimpleBulks.BulkUpdate")
    {
        TableMapper.Register(typeof(SingleKeyRow<int>), _schema, "SingleKeyRows");
        TableMapper.Register(typeof(CompositeKeyRow<int, int>), _schema, "CompositeKeyRows");

        var tran = _context.Database.BeginTransaction();

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

        tran.Commit();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Bulk_Update_Without_Transaction(bool useLinq, bool omitTableName)
    {
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

        if (useLinq)
        {
            if (omitTableName)
            {
                _connection.BulkUpdate(rows,
                    row => row.Id,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkUpdate(compositeKeyRows,
                    row => new { row.Id1, row.Id2 },
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkUpdate(rows, new TableInfor(_schema, "SingleKeyRows"),
                    row => row.Id,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkUpdate(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    row => new { row.Id1, row.Id2 },
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

            var newIndex = rows.Max(x => x.Id) + 1;

            var bulkId = SequentialGuidGenerator.Next();

            rows.Add(new SingleKeyRow<int>
            {
                Column1 = newIndex,
                Column2 = "Inserted using Merge" + newIndex,
                Column3 = DateTime.Now,
                BulkId = bulkId
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = newId1,
                Id2 = newId2,
                Column1 = newId2,
                Column2 = "Inserted using Merge" + newId2,
                Column3 = DateTime.Now,
            });

            if (omitTableName)
            {
                _connection.BulkMerge(rows,
                    row => row.Id,
                    row => new { row.Column1, row.Column2 },
                    row => new { row.Column1, row.Column2, row.Column3, row.BulkId },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkMerge(compositeKeyRows,
                    row => new { row.Id1, row.Id2 },
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkMerge(rows, new TableInfor(_schema, "SingleKeyRows"),
                    row => row.Id,
                    row => new { row.Column1, row.Column2 },
                    row => new { row.Column1, row.Column2, row.Column3, row.BulkId },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkMerge(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    row => new { row.Id1, row.Id2 },
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

            rows.First(x => x.BulkId == bulkId).Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();

        }
        else
        {
            if (omitTableName)
            {
                _connection.BulkUpdate(rows,
                    "Id",
                    ["Column3", "Column2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkUpdate(compositeKeyRows,
                    ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkUpdate(rows, new TableInfor(_schema, "SingleKeyRows"),
                    "Id",
                    ["Column3", "Column2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkUpdate(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

            var newIndex = rows.Max(x => x.Id) + 1;

            var bulkId = SequentialGuidGenerator.Next();

            rows.Add(new SingleKeyRow<int>
            {
                Column1 = newIndex,
                Column2 = "Inserted using Merge" + newIndex,
                Column3 = DateTime.Now,
                BulkId = bulkId
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = newId1,
                Id2 = newId2,
                Column1 = newId2,
                Column2 = "Inserted using Merge" + newId2,
                Column3 = DateTime.Now,
            });

            if (omitTableName)
            {
                _connection.BulkMerge(rows,
                    "Id",
                    ["Column1", "Column2"],
                    ["Column1", "Column2", "Column3", "BulkId"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkMerge(compositeKeyRows,
                    ["Id1", "Id2"],
                    ["Column1", "Column2", "Column3"],
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }
            else
            {
                _connection.BulkMerge(rows, new TableInfor(_schema, "SingleKeyRows"),
                    "Id",
                    ["Column1", "Column2"],
                    ["Column1", "Column2", "Column3", "BulkId"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

                _connection.BulkMerge(compositeKeyRows, new TableInfor(_schema, "CompositeKeyRows"),
                    ["Id1", "Id2"],
                    ["Column1", "Column2", "Column3"],
                    ["Id1", "Id2", "Column1", "Column2", "Column3"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            }

            rows.First(x => x.BulkId == bulkId).Id = _context.SingleKeyRows.Where(x => x.BulkId == bulkId).Select(x => x.Id).FirstOrDefault();
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (int i = 0; i < 101; i++)
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