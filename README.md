# EntityFrameworkCore.MySql.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **COPY FROM STDIN** command.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance **DbContextExtensions.cs**
or you can use **MySqlConnectionExtensions.cs** to work directly with a **MySqlConnection** instance without using EntityFrameworkCore.

## Nuget
https://www.nuget.org/packages/EntityFrameworkCore.MySql.SimpleBulks

## Features
- Bulk Insert
- Bulk Update
- Bulk Delete
- Bulk Merge
- Bulk Match
- Temp Table
- Direct Insert
- Direct Update
- Direct Delete

## Examples
[EntityFrameworkCore.MySql.SimpleBulks.Demo](/src/EntityFrameworkCore.MySql.SimpleBulks.Demo/Program.cs)
- Update the connection string:
  ```c#
  private const string _connectionString = "server=localhost;database=SimpleBulks;user=root;password=mysql;AllowLoadLocalInfile=true";
  ```
- Build and run.

## DbContextExtensions
### Using Lambda Expression
```c#
using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

// Insert all columns
dbct.BulkInsert(rows);
dbct.BulkInsert(compositeKeyRows);

// Insert selected columns only
dbct.BulkInsert(rows,
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkInsert(compositeKeyRows,
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

dbct.BulkUpdate(rows,
    row => new { row.Column3, row.Column2 });
dbct.BulkUpdate(compositeKeyRows,
    row => new { row.Column3, row.Column2 });

dbct.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkMerge(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
dbct.BulkDelete(rows);
dbct.BulkDelete(compositeKeyRows);
```
### Using Dynamic String
```c#
using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

dbct.BulkUpdate(rows,
    [ "Column3", "Column2" ]);
dbct.BulkUpdate(compositeKeyRows,
    [ "Column3", "Column2" ]);

dbct.BulkMerge(rows,
    "Id",
    [ "Column1", "Column2" ],
    [ "Column1", "Column2", "Column3" ]);
dbct.BulkMerge(compositeKeyRows,
    [ "Id1", "Id2" ],
    [ "Column1", "Column2", "Column3" ],
    [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression
```c#
new BulkInsertBuilder<Row>(dbct.GetMySqlConnection())
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(dbct.GetTableInfor(typeof(Row)))
	// or .ToTable("Rows")
	.Execute(rows);
```

## MySqlConnectionExtensions
### Using Lambda Expression
```c#
using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

// Register Type - Table Name globaly
TableMapper.Register(typeof(Row), "Rows");
TableMapper.Register(typeof(CompositeKeyRow), "CompositeKeyRows");

connection.BulkInsert(rows,
           row => new { row.Column1, row.Column2, row.Column3 });
connection.BulkInsert(compositeKeyRows,
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

connection.BulkUpdate(rows,
           row => row.Id,
           row => new { row.Column3, row.Column2 });
connection.BulkUpdate(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column3, row.Column2 });

connection.BulkMerge(rows,
           row => row.Id,
           row => new { row.Column1, row.Column2 },
           row => new { row.Column1, row.Column2, row.Column3 });
connection.BulkMerge(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
connection.BulkDelete(rows, row => row.Id);
connection.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
```
### Using Dynamic String
```c#
using EntityFrameworkCore.MySql.SimpleBulks.BulkDelete;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkMerge;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;

connection.BulkInsert(rows, "Rows",
           [ "Column1", "Column2", "Column3" ]);
connection.BulkInsert(rows.Take(1000), "Rows",
           typeof(Row).GetDbColumnNames("Id"));
connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

connection.BulkUpdate(rows, "Rows",
           "Id",
           [ "Column3", "Column2" ]);
connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2" ],
           [ "Column3", "Column2" ]);

connection.BulkMerge(rows, "Rows",
           "Id",
           [ "Column1", "Column2" ],
           [ "Column1", "Column2", "Column3" ]);
connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2" ],
           [ "Column1", "Column2", "Column3" ],
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

connection.BulkDelete(rows, "Rows", "Id");
connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", [ "Id1", "Id2" ]);
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression
```c#
new BulkInsertBuilder<Row>(connection)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable("Rows")
	.Execute(rows);
```

## Configuration
### BulkInsert
```c#
_context.BulkInsert(rows,
    row => new { row.Column1, row.Column2, row.Column3 },
    options =>
    {
        options.KeepIdentity = false;
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### BulkUpdate
```c#
_context.BulkUpdate(rows,
    row => new { row.Column3, row.Column2 },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### BulkDelete
```c#
_context.BulkDelete(rows,
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### BulkMerge
```c#
_context.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### BulkMatch
```c#
var contactsFromDb = _context.BulkMatch(matchedContacts,
    x => new { x.CustomerId, x.CountryIsoCode },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### TempTable
```c#
var customerTableName = _context.CreateTempTable(customers,
    x => new
    {
        x.IdNumber,
        x.FirstName,
        x.LastName,
        x.CurrentCountryIsoCode
    },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectInsert
```c#
_context.DirectInsert(row,
    row => new { row.Column1, row.Column2, row.Column3 },
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectUpdate
```c#
_context.DirectUpdate(row,
    row => new { row.Column3, row.Column2 },
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectDelete
```c#
_context.DirectDelete(row,
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```

## Returned Result
### BulkUpdate
```c#
var updateResult = dbct.BulkUpdate(rows, row => new { row.Column3, row.Column2 });

Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");
```
### BulkDelete
```c#
var deleteResult = dbct.BulkDelete(rows);

Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");
```
### BulkMerge
```c#
var mergeResult = dbct.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });

Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");
```

## License
**EntityFrameworkCore.MySql.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
