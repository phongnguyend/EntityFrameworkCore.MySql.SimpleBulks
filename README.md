# EntityFrameworkCore.MySql.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **MySqlBulkCopy** class.
 
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

## Benchmarks
### BulkInsert
Single Table [/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-RGIUAC : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|----------:|--------------:|
| **EFCoreInsert** |       **100** |     **36.86 ms** |    **NA** |           **-** |           **-** |         **-** |     **976.73 KB** |
|   BulkInsert |       100 |     18.73 ms |    NA |           - |           - |         - |      56.31 KB |
| **EFCoreInsert** |      **1000** |    **146.98 ms** |    **NA** |   **1000.0000** |           **-** |         **-** |    **9740.39 KB** |
|   BulkInsert |      1000 |    112.55 ms |    NA |           - |           - |         - |     312.73 KB |
| **EFCoreInsert** |     **10000** |  **1,247.58 ms** |    **NA** |  **15000.0000** |   **5000.0000** |         **-** |   **96372.25 KB** |
|   BulkInsert |     10000 |    531.13 ms |    NA |           - |           - |         - |    2902.21 KB |
| **EFCoreInsert** |    **100000** |  **5,455.73 ms** |    **NA** | **147000.0000** |  **37000.0000** |         **-** |  **956706.58 KB** |
|   BulkInsert |    100000 |  1,267.28 ms |    NA |   3000.0000 |   1000.0000 |         - |   28606.81 KB |
| **EFCoreInsert** |    **250000** | **13,465.43 ms** |    **NA** | **368000.0000** |  **88000.0000** |         **-** | **2368616.45 KB** |
|   BulkInsert |    250000 |  2,606.24 ms |    NA |   9000.0000 |   5000.0000 | 1000.0000 |   71551.59 KB |
| **EFCoreInsert** |    **500000** | **27,001.92 ms** |    **NA** | **736000.0000** | **174000.0000** |         **-** |  **4744491.9 KB** |
|   BulkInsert |    500000 |  4,806.86 ms |    NA |  17000.0000 |   7000.0000 |         - |  142890.45 KB |


Multiple Tables (1x parent rows + 5x child rows) [/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-XRWCCS : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |        Mean | Error |         Gen0 |        Gen1 |      Gen2 |    Allocated |
|------------- |---------- |------------:|------:|-------------:|------------:|----------:|-------------:|
| **EFCoreInsert** |       **100** |    **189.6 ms** |    **NA** |    **1000.0000** |           **-** |         **-** |   **6875.61 KB** |
|   BulkInsert |       100 |    166.7 ms |    NA |            - |           - |         - |    271.06 KB |
| **EFCoreInsert** |      **1000** |  **1,097.2 ms** |    **NA** |   **10000.0000** |   **4000.0000** |         **-** |   **67968.5 KB** |
|   BulkInsert |      1000 |    721.5 ms |    NA |            - |           - |         - |   1987.01 KB |
| **EFCoreInsert** |     **10000** |  **4,663.4 ms** |    **NA** |  **105000.0000** |  **29000.0000** |         **-** | **675868.05 KB** |
|   BulkInsert |     10000 |  1,789.9 ms |    NA |    3000.0000 |   1000.0000 | 1000.0000 |  19330.64 KB |
| **EFCoreInsert** |    **100000** | **43,841.6 ms** |    **NA** | **1050000.0000** | **250000.0000** |         **-** | **6740771.2 KB** |
|   BulkInsert |    100000 | 19,456.4 ms |    NA |   25000.0000 |  12000.0000 | 1000.0000 | 193110.47 KB |


### BulkUpdate
[/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-JZFEWQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|--------------:|
| **EFCoreUpdate** |       **100** |     **49.92 ms** |    **NA** |           **-** |          **-** |     **860.98 KB** |
|   BulkUpdate |       100 |     20.90 ms |    NA |           - |          - |      56.61 KB |
| **EFCoreUpdate** |      **1000** |    **327.28 ms** |    **NA** |   **1000.0000** |          **-** |    **8284.28 KB** |
|   BulkUpdate |      1000 |     72.71 ms |    NA |           - |          - |     336.83 KB |
| **EFCoreUpdate** |     **10000** |  **2,609.00 ms** |    **NA** |  **12000.0000** |  **3000.0000** |   **81612.44 KB** |
|   BulkUpdate |     10000 |    320.06 ms |    NA |           - |          - |    3105.73 KB |
| **EFCoreUpdate** |    **100000** | **26,402.28 ms** |    **NA** | **122000.0000** | **29000.0000** |   **808508.6 KB** |
|   BulkUpdate |    100000 |  6,449.05 ms |    NA |   4000.0000 |  1000.0000 |   29886.09 KB |
| **EFCoreUpdate** |    **250000** | **69,878.17 ms** |    **NA** | **306000.0000** | **70000.0000** | **2001389.61 KB** |
|   BulkUpdate |    250000 | 31,401.96 ms |    NA |  12000.0000 |  6000.0000 |   74524.23 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-JZFEWQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |    Mean | Error |       Gen0 |       Gen1 | Allocated |
|----------- |---------- |--------:|------:|-----------:|-----------:|----------:|
| **BulkUpdate** |    **500000** | **114.1 s** |    **NA** | **24000.0000** | **15000.0000** | **145.43 MB** |
| **BulkUpdate** |   **1000000** | **435.8 s** |    **NA** | **48000.0000** | **32000.0000** | **290.75 MB** |


### BulkDelete
[/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-CDMQUJ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |       Gen0 |       Gen1 |    Allocated |
|------------- |---------- |-------------:|------:|-----------:|-----------:|-------------:|
| **EFCoreDelete** |       **100** |     **36.94 ms** |    **NA** |          **-** |          **-** |    **682.91 KB** |
|   BulkDelete |       100 |     24.97 ms |    NA |          - |          - |     37.79 KB |
| **EFCoreDelete** |      **1000** |    **231.29 ms** |    **NA** |  **1000.0000** |  **1000.0000** |   **6762.27 KB** |
|   BulkDelete |      1000 |     32.78 ms |    NA |          - |          - |     202.8 KB |
| **EFCoreDelete** |     **10000** |  **2,209.83 ms** |    **NA** | **10000.0000** |  **3000.0000** |  **66219.48 KB** |
|   BulkDelete |     10000 |    193.91 ms |    NA |          - |          - |   1835.73 KB |
| **EFCoreDelete** |     **20000** |  **4,705.65 ms** |    **NA** | **20000.0000** |  **8000.0000** | **132780.13 KB** |
|   BulkDelete |     20000 |    376.96 ms |    NA |          - |          - |    3648.2 KB |
| **EFCoreDelete** |     **50000** | **11,842.03 ms** |    **NA** | **49000.0000** | **14000.0000** | **327102.23 KB** |
|   BulkDelete |     50000 |  1,005.48 ms |    NA |  1000.0000 |          - |    9029.6 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-UIDALO : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |     Mean | Error |       Gen0 |       Gen1 | Allocated |
|----------- |---------- |---------:|------:|-----------:|-----------:|----------:|
| **BulkDelete** |    **100000** |  **1.963 s** |    **NA** |  **2000.0000** |  **1000.0000** |  **17.61 MB** |
| **BulkDelete** |    **250000** |  **6.787 s** |    **NA** |  **5000.0000** |  **2000.0000** |  **44.06 MB** |
| **BulkDelete** |    **500000** | **43.473 s** |    **NA** | **10000.0000** |  **5000.0000** |  **87.97 MB** |
| **BulkDelete** |   **1000000** | **75.212 s** |    **NA** | **21000.0000** | **10000.0000** | **176.77 MB** |


### BulkMerge
[/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-UIDALO : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|----------:|--------------:|
| **EFCoreUpsert** |       **100** |     **74.94 ms** |    **NA** |           **-** |           **-** |         **-** |    **1824.88 KB** |
|    BulkMerge |       100 |     38.65 ms |    NA |           - |           - |         - |     103.85 KB |
| **EFCoreUpsert** |      **1000** |    **393.71 ms** |    **NA** |   **2000.0000** |   **1000.0000** |         **-** |   **17861.01 KB** |
|    BulkMerge |      1000 |    178.14 ms |    NA |           - |           - |         - |      677.6 KB |
| **EFCoreUpsert** |     **10000** |  **3,193.35 ms** |    **NA** |  **27000.0000** |   **8000.0000** |         **-** |  **178255.24 KB** |
|    BulkMerge |     10000 |    549.66 ms |    NA |           - |           - |         - |    6450.78 KB |
| **EFCoreUpsert** |    **100000** | **34,100.63 ms** |    **NA** | **269000.0000** |  **63000.0000** |         **-** | **1767492.45 KB** |
|    BulkMerge |    100000 |  5,938.77 ms |    NA |   9000.0000 |   5000.0000 | 1000.0000 |   63979.73 KB |
| **EFCoreUpsert** |    **250000** | **90,654.47 ms** |    **NA** | **674000.0000** | **153000.0000** |         **-** | **4375295.44 KB** |
|    BulkMerge |    250000 | 31,726.66 ms |    NA |  21000.0000 |  11000.0000 | 1000.0000 |   162691.7 KB |


### BulkMatch
Single Column [/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-XRORYQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |          Mean | Error |        Gen0 |       Gen1 |      Gen2 |    Allocated |
|------------------ |---------- |--------------:|------:|------------:|-----------:|----------:|-------------:|
|      **EFCoreSelect** |       **100** |     **87.293 ms** |    **NA** |           **-** |          **-** |         **-** |    **755.39 KB** |
| EFCoreBatchSelect |       100 |      5.472 ms |    NA |           - |          - |         - |    112.87 KB |
|         BulkMatch |       100 |      6.441 ms |    NA |           - |          - |         - |     94.54 KB |
|      **EFCoreSelect** |      **1000** |    **943.840 ms** |    **NA** |   **1000.0000** |          **-** |         **-** |   **7486.88 KB** |
| EFCoreBatchSelect |      1000 |     14.235 ms |    NA |           - |          - |         - |    899.45 KB |
|         BulkMatch |      1000 |     15.678 ms |    NA |           - |          - |         - |    730.93 KB |
|      **EFCoreSelect** |     **10000** |  **8,673.825 ms** |    **NA** |  **12000.0000** |  **1000.0000** |         **-** |   **74547.2 KB** |
| EFCoreBatchSelect |     10000 |    145.653 ms |    NA |   1000.0000 |          - |         - |   9060.32 KB |
|         BulkMatch |     10000 |    138.112 ms |    NA |   1000.0000 |          - |         - |   7315.17 KB |
|      **EFCoreSelect** |    **100000** | **77,525.831 ms** |    **NA** | **122000.0000** | **25000.0000** | **1000.0000** | **745727.41 KB** |
| EFCoreBatchSelect |    100000 |    713.308 ms |    NA |  13000.0000 |  5000.0000 | 1000.0000 |  90699.73 KB |
|         BulkMatch |    100000 |    739.911 ms |    NA |  12000.0000 |  5000.0000 | 1000.0000 |  73822.48 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-XRORYQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |    Mean | Error |        Gen0 |       Gen1 |      Gen2 | Allocated |
|------------------ |---------- |--------:|------:|------------:|-----------:|----------:|----------:|
| **EFCoreBatchSelect** |    **250000** | **2.086 s** |    **NA** |  **32000.0000** | **12000.0000** | **1000.0000** | **220.07 MB** |
|         BulkMatch |    250000 | 1.785 s |    NA |  28000.0000 | 11000.0000 | 1000.0000 | 179.51 MB |
| **EFCoreBatchSelect** |    **500000** | **4.617 s** |    **NA** |  **63000.0000** | **23000.0000** | **1000.0000** | **440.05 MB** |
|         BulkMatch |    500000 | 3.884 s |    NA |  55000.0000 | 22000.0000 | 1000.0000 | 359.01 MB |
| **EFCoreBatchSelect** |   **1000000** | **8.081 s** |    **NA** | **125000.0000** | **49000.0000** | **1000.0000** | **880.01 MB** |
|         BulkMatch |   1000000 | 8.634 s |    NA | 108000.0000 | 42000.0000 | 1000.0000 | 718.02 MB |


Multiple Columns [/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-XRORYQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |    Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|-------------:|
| **EFCoreSelect** |       **100** |    **167.60 ms** |    **NA** |           **-** |          **-** |   **1002.69 KB** |
|    BulkMatch |       100 |     11.01 ms |    NA |           - |          - |    152.36 KB |
| **EFCoreSelect** |      **1000** |  **1,239.99 ms** |    **NA** |   **1000.0000** |          **-** |   **9452.78 KB** |
|    BulkMatch |      1000 |     35.72 ms |    NA |           - |          - |   1188.09 KB |
| **EFCoreSelect** |     **10000** | **11,437.62 ms** |    **NA** |  **15000.0000** |  **3000.0000** |  **97825.21 KB** |
|    BulkMatch |     10000 |    289.64 ms |    NA |   1000.0000 |          - |  11953.45 KB |
| **EFCoreSelect** |    **100000** | **90,754.18 ms** |    **NA** | **154000.0000** | **38000.0000** | **948693.23 KB** |
|    BulkMatch |    100000 |  1,981.62 ms |    NA |  18000.0000 |  6000.0000 | 120447.04 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-XRORYQ : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|    Method | RowsCount |     Mean | Error |        Gen0 |       Gen1 |      Gen2 |  Allocated |
|---------- |---------- |---------:|------:|------------:|-----------:|----------:|-----------:|
| **BulkMatch** |    **250000** |  **6.536 s** |    **NA** |  **46000.0000** | **16000.0000** |         **-** |  **296.15 MB** |
| **BulkMatch** |    **500000** | **11.922 s** |    **NA** |  **92000.0000** | **32000.0000** |         **-** |  **594.47 MB** |
| **BulkMatch** |   **1000000** | **40.775 s** |    **NA** | **185000.0000** | **66000.0000** | **1000.0000** | **1194.94 MB** |


### TempTable
[/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/TempTableBenchmarks.cs](/src/EntityFrameworkCore.MySql.SimpleBulks.Benchmarks/TempTableBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-GTTKRK : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|          Method | RowsCount |         Mean | Error |       Gen0 |       Gen1 |      Gen2 |    Allocated |
|---------------- |---------- |-------------:|------:|-----------:|-----------:|----------:|-------------:|
| **CreateTempTable** |       **100** |     **5.322 ms** |    **NA** |          **-** |          **-** |         **-** |     **56.78 KB** |
| **CreateTempTable** |      **1000** |    **14.892 ms** |    **NA** |          **-** |          **-** |         **-** |    **285.05 KB** |
| **CreateTempTable** |     **10000** |    **90.559 ms** |    **NA** |          **-** |          **-** |         **-** |   **2593.26 KB** |
| **CreateTempTable** |    **100000** |   **566.383 ms** |    **NA** |  **3000.0000** |  **1000.0000** |         **-** |  **25485.38 KB** |
| **CreateTempTable** |    **250000** | **1,039.989 ms** |    **NA** |  **8000.0000** |  **4000.0000** | **1000.0000** |  **63742.06 KB** |
| **CreateTempTable** |    **500000** | **2,069.624 ms** |    **NA** | **15000.0000** |  **7000.0000** | **1000.0000** | **127270.92 KB** |
| **CreateTempTable** |   **1000000** | **4,251.546 ms** |    **NA** | **29000.0000** | **14000.0000** | **1000.0000** | **254340.69 KB** |


## License
**EntityFrameworkCore.MySql.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
