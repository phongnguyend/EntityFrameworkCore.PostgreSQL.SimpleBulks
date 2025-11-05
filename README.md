# EntityFrameworkCore.PostgreSQL.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **COPY FROM STDIN** command.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance **DbContextExtensions.cs**
or you can use **ConnectionContextExtensions.cs** to work directly with a **NpgsqlConnection** instance without using EntityFrameworkCore.

## Nuget
| Database | Package | GitHub |
| -------- | ------- | ------ |
| SQL Server| [EntityFrameworkCore.SqlServer.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.SqlServer.SimpleBulks) | [EntityFrameworkCore.SqlServer.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.SqlServer.SimpleBulks) |
| PostgreSQL| [EntityFrameworkCore.PostgreSQL.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.PostgreSQL.SimpleBulks) | [EntityFrameworkCore.PostgreSQL.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.PostgreSQL.SimpleBulks) |
| MySQL| [EntityFrameworkCore.MySQL.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.MySQL.SimpleBulks) | [EntityFrameworkCore.MySQL.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.MySQL.SimpleBulks) |

## Features
- Bulk Insert (requires PostgreSQL >= 17)
- Bulk Update
- Bulk Delete
- Bulk Merge (requires PostgreSQL >= 17)
- Bulk Match
- Temp Table
- Direct Insert
- Direct Update
- Direct Delete
- Upsert

## Examples
[EntityFrameworkCore.PostgreSQL.SimpleBulks.Demo](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Demo/Program.cs)
- Update the connection string:
  ```c#
  private const string _connectionString = "Host=127.0.0.1;Database=SimpleBulks;Username=postgres;Password=postgres";
  ```
- Build and run.

## DbContextExtensions
### Using Lambda Expression
```c#
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

// Insert all columns
await dbct.BulkInsertAsync(rows);
await dbct.BulkInsertAsync(compositeKeyRows);

// Insert selected columns only
await dbct.BulkInsertAsync(rows,
    row => new { row.Column1, row.Column2, row.Column3 });
await dbct.BulkInsertAsync(compositeKeyRows,
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

await dbct.BulkUpdateAsync(rows,
    row => new { row.Column3, row.Column2 });
await dbct.BulkUpdateAsync(compositeKeyRows,
    row => new { row.Column3, row.Column2 });

await dbct.BulkMergeAsync(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });
await dbct.BulkMergeAsync(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
await dbct.BulkDeleteAsync(rows);
await dbct.BulkDeleteAsync(compositeKeyRows);
```
### Using Dynamic String
```c#
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

await dbct.BulkUpdateAsync(rows,
    [ "Column3", "Column2" ]);
await dbct.BulkUpdateAsync(compositeKeyRows,
    [ "Column3", "Column2" ]);

await dbct.BulkMergeAsync(rows,
    ["Id"],
    [ "Column1", "Column2" ],
    [ "Column1", "Column2", "Column3" ]);
await dbct.BulkMergeAsync(compositeKeyRows,
    [ "Id1", "Id2" ],
    [ "Column1", "Column2", "Column3" ],
    [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);
```
### Using Builder Approach in case you need both Dynamic & Lambda Expression
```c#
await dbct.CreateBulkInsertBuilder<Row>()
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(dbct.GetTableInfor(typeof(Row)))
	.ExecuteAsync(rows);
```

## ConnectionContextExtensions
### Using Lambda Expression
```c#
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

// Register Type - Table Name globaly
TableMapper.Register(typeof(Row), new NpgsqlTableInfor("Rows"));
TableMapper.Register(typeof(CompositeKeyRow), new NpgsqlTableInfor("CompositeKeyRows"));

var connection = new ConnectionContext(new NpgsqlConnection(connectionString), null);

await connection.BulkInsertAsync(rows,
           row => new { row.Column1, row.Column2, row.Column3 });
await connection.BulkInsertAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

await connection.BulkUpdateAsync(rows,
           row => row.Id,
           row => new { row.Column3, row.Column2 });
await connection.BulkUpdateAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column3, row.Column2 });

await connection.BulkMergeAsync(rows,
           row => row.Id,
           row => new { row.Column1, row.Column2 },
           row => new { row.Column1, row.Column2, row.Column3 });
await connection.BulkMergeAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
await connection.BulkDeleteAsync(rows, row => row.Id);
await connection.BulkDeleteAsync(compositeKeyRows, row => new { row.Id1, row.Id2 });
```
### Using Dynamic String
```c#
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkMerge;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkUpdate;

var connection = new ConnectionContext(new NpgsqlConnection(connectionString), null);

await connection.BulkInsertAsync(rows,
           [ "Column1", "Column2", "Column3" ]);
await connection.BulkInsertAsync(compositeKeyRows,
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

await connection.BulkUpdateAsync(rows,
           ["Id"],
           [ "Column3", "Column2" ]);
await connection.BulkUpdateAsync(compositeKeyRows,
           [ "Id1", "Id2" ],
           [ "Column3", "Column2" ]);

await connection.BulkMergeAsync(rows,
           ["Id"],
           [ "Column1", "Column2" ],
           [ "Column1", "Column2", "Column3" ]);
await connection.BulkMergeAsync(compositeKeyRows,
           [ "Id1", "Id2" ],
           [ "Column1", "Column2", "Column3" ],
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

await connection.BulkDeleteAsync(rows, ["Id"]);
await connection.BulkDeleteAsync(compositeKeyRows, [ "Id1", "Id2" ]);
```
### Using Builder Approach in case you need both Dynamic & Lambda Expression
```c#
await connection.CreateBulkInsertBuilder<Row>()
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(new NpgsqlTableInfor("Rows"))
	.ExecuteAsync(rows);
```

## Execution Options
### BulkInsert
```c#
await _context.BulkInsertAsync(rows,
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkInsertOptions
    {
        KeepIdentity = false,
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkUpdate
```c#
await _context.BulkUpdateAsync(rows,
    row => new { row.Column3, row.Column2 },
    new BulkUpdateOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkDelete
```c#
await _context.BulkDeleteAsync(rows,
    new BulkDeleteOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkMerge
```c#
await _context.BulkMergeAsync(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkMergeOptions
    {
        BatchSize = 0,
        Timeout = 30,
        ReturnDbGeneratedId = true,
        LogTo = Console.WriteLine
    });
```
### BulkMatch
```c#
var contactsFromDb = await _context.BulkMatchAsync(matchedContacts,
    x => new { x.CustomerId, x.CountryIsoCode },
    new BulkMatchOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### TempTable
```c#
var customerTableName = await _context.CreateTempTableAsync(customers,
    x => new
    {
        x.IdNumber,
        x.FirstName,
        x.LastName,
        x.CurrentCountryIsoCode
    },
    new TempTableOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectInsert
```c#
await _context.DirectInsertAsync(row,
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkInsertOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectUpdate
```c#
await _context.DirectUpdateAsync(row,
    row => new { row.Column3, row.Column2 },
    new BulkUpdateOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectDelete
```c#
await _context.DirectDeleteAsync(row,
    new BulkDeleteOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```

## Returned Result
### BulkUpdate
```c#
var updateResult = await dbct.BulkUpdateAsync(rows, row => new { row.Column3, row.Column2 });

Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");
```
### BulkDelete
```c#
var deleteResult = await dbct.BulkDeleteAsync(rows);

Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");
```
### BulkMerge
```c#
var mergeResult = await dbct.BulkMergeAsync(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });

Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");
```

## Benchmarks
### BulkInsert
Single Table [/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-VUKETC : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|----------:|--------------:|
| **EFCoreInsert** |       **100** |     **24.87 ms** |    **NA** |           **-** |           **-** |         **-** |    **1117.16 KB** |
|   BulkInsert |       100 |     16.19 ms |    NA |           - |           - |         - |      144.4 KB |
| **EFCoreInsert** |      **1000** |    **116.08 ms** |    **NA** |   **1000.0000** |   **1000.0000** |         **-** |   **10733.77 KB** |
|   BulkInsert |      1000 |     26.10 ms |    NA |           - |           - |         - |     909.45 KB |
| **EFCoreInsert** |     **10000** |  **1,032.25 ms** |    **NA** |  **14000.0000** |   **6000.0000** |         **-** |  **101319.22 KB** |
|   BulkInsert |     10000 |    158.03 ms |    NA |   1000.0000 |   1000.0000 |         - |    8484.25 KB |
| **EFCoreInsert** |    **100000** |  **6,424.85 ms** |    **NA** | **138000.0000** |  **50000.0000** |         **-** |  **997459.33 KB** |
|   BulkInsert |    100000 |    851.58 ms |    NA |  12000.0000 |   2000.0000 | 1000.0000 |   78651.35 KB |
| **EFCoreInsert** |    **250000** | **15,169.42 ms** |    **NA** | **346000.0000** | **121000.0000** |         **-** | **2469857.71 KB** |
|   BulkInsert |    250000 |  2,049.98 ms |    NA |  28000.0000 |   1000.0000 |         - |  193035.99 KB |
| **EFCoreInsert** |    **500000** | **34,492.82 ms** |    **NA** | **693000.0000** | **240000.0000** |         **-** | **4946532.22 KB** |
|   BulkInsert |    500000 |  3,977.10 ms |    NA |  57000.0000 |   2000.0000 |         - |  387290.34 KB |


Multiple Tables (1x parent rows + 5x child rows) [/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LTFAOU : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |         Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|-------------:|------------:|----------:|--------------:|
| **EFCoreInsert** |       **100** |    **145.08 ms** |    **NA** |    **1000.0000** |   **1000.0000** |         **-** |    **8343.67 KB** |
|   BulkInsert |       100 |     49.57 ms |    NA |            - |           - |         - |      665.8 KB |
| **EFCoreInsert** |      **1000** |    **795.13 ms** |    **NA** |   **11000.0000** |   **5000.0000** |         **-** |   **78542.85 KB** |
|   BulkInsert |      1000 |    188.83 ms |    NA |            - |           - |         - |    5941.84 KB |
| **EFCoreInsert** |     **10000** |  **6,722.98 ms** |    **NA** |  **109000.0000** |  **41000.0000** |         **-** |  **772122.11 KB** |
|   BulkInsert |     10000 |  1,302.40 ms |    NA |    9000.0000 |   2000.0000 | 1000.0000 |   55370.05 KB |
| **EFCoreInsert** |    **100000** | **61,476.01 ms** |    **NA** | **1087000.0000** | **373000.0000** |         **-** | **7687142.55 KB** |
|   BulkInsert |    100000 | 12,805.28 ms |    NA |   82000.0000 |   4000.0000 |         - |  548065.25 KB |


### BulkUpdate
[/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|--------------:|
| **EFCoreUpdate** |       **100** |     **26.56 ms** |    **NA** |           **-** |          **-** |    **1056.89 KB** |
|   BulkUpdate |       100 |     20.16 ms |    NA |           - |          - |      53.34 KB |
| **EFCoreUpdate** |      **1000** |    **118.31 ms** |    **NA** |   **1000.0000** |  **1000.0000** |    **8172.52 KB** |
|   BulkUpdate |      1000 |     29.87 ms |    NA |           - |          - |     415.93 KB |
| **EFCoreUpdate** |     **10000** |  **1,000.87 ms** |    **NA** |  **10000.0000** |  **3000.0000** |   **75273.42 KB** |
|   BulkUpdate |     10000 |    175.40 ms |    NA |           - |          - |    4012.16 KB |
| **EFCoreUpdate** |    **100000** |  **7,526.76 ms** |    **NA** | **104000.0000** | **28000.0000** |  **735520.32 KB** |
|   BulkUpdate |    100000 |  1,395.47 ms |    NA |   6000.0000 |  1000.0000 |   38947.02 KB |
| **EFCoreUpdate** |    **250000** | **18,029.63 ms** |    **NA** | **260000.0000** | **70000.0000** | **1818097.27 KB** |
|   BulkUpdate |    250000 |  4,606.96 ms |    NA |  15000.0000 |  3000.0000 |   97174.13 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-FVGKFX : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |    Mean | Error |       Gen0 |       Gen1 | Allocated |
|----------- |---------- |--------:|------:|-----------:|-----------:|----------:|
| **BulkUpdate** |    **500000** | **14.99 s** |    **NA** | **31000.0000** |  **6000.0000** | **189.67 MB** |
| **BulkUpdate** |   **1000000** | **52.35 s** |    **NA** | **63000.0000** | **13000.0000** | **379.21 MB** |


### BulkDelete
[/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |        Mean | Error |       Gen0 |       Gen1 |    Allocated |
|------------- |---------- |------------:|------:|-----------:|-----------:|-------------:|
| **EFCoreDelete** |       **100** |    **18.97 ms** |    **NA** |          **-** |          **-** |     **675.7 KB** |
|   BulkDelete |       100 |    16.18 ms |    NA |          - |          - |     25.73 KB |
| **EFCoreDelete** |      **1000** |   **127.17 ms** |    **NA** |  **1000.0000** |  **1000.0000** |   **6448.29 KB** |
|   BulkDelete |      1000 |    41.02 ms |    NA |          - |          - |    166.36 KB |
| **EFCoreDelete** |     **10000** |   **941.10 ms** |    **NA** |  **9000.0000** |  **2000.0000** |  **61677.63 KB** |
|   BulkDelete |     10000 |   209.25 ms |    NA |          - |          - |   1572.33 KB |
| **EFCoreDelete** |     **20000** | **2,188.30 ms** |    **NA** | **17000.0000** |  **6000.0000** | **122600.81 KB** |
|   BulkDelete |     20000 |   324.00 ms |    NA |          - |          - |    3135.3 KB |
| **EFCoreDelete** |     **50000** | **3,430.29 ms** |    **NA** | **43000.0000** | **11000.0000** |  **301041.3 KB** |
|   BulkDelete |     50000 |   756.71 ms |    NA |  1000.0000 |          - |   7822.52 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-RRDFUF : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |     Mean | Error |       Gen0 |      Gen1 | Allocated |
|----------- |---------- |---------:|------:|-----------:|----------:|----------:|
| **BulkDelete** |    **100000** |  **2.108 s** |    **NA** |  **2000.0000** | **1000.0000** |  **15.27 MB** |
| **BulkDelete** |    **250000** |  **3.905 s** |    **NA** |  **6000.0000** | **1000.0000** |  **38.16 MB** |
| **BulkDelete** |    **500000** |  **7.768 s** |    **NA** | **12000.0000** | **1000.0000** |   **76.3 MB** |
| **BulkDelete** |   **1000000** | **15.895 s** |    **NA** | **25000.0000** | **1000.0000** |  **152.6 MB** |


### BulkMerge
[/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|--------------:|
| **EFCoreUpsert** |       **100** |     **39.72 ms** |    **NA** |           **-** |           **-** |    **2138.82 KB** |
|    BulkMerge |       100 |     31.75 ms |    NA |           - |           - |     201.27 KB |
| **EFCoreUpsert** |      **1000** |    **446.34 ms** |    **NA** |   **2000.0000** |   **1000.0000** |   **18676.95 KB** |
|    BulkMerge |      1000 |     60.98 ms |    NA |           - |           - |    1821.43 KB |
| **EFCoreUpsert** |     **10000** |  **2,293.35 ms** |    **NA** |  **24000.0000** |  **10000.0000** |  **175741.33 KB** |
|    BulkMerge |     10000 |    315.95 ms |    NA |   2000.0000 |   1000.0000 |   17002.34 KB |
| **EFCoreUpsert** |    **100000** | **14,094.90 ms** |    **NA** | **243000.0000** |  **75000.0000** | **1735002.65 KB** |
|    BulkMerge |    100000 |  1,933.37 ms |    NA |  26000.0000 |   1000.0000 |  168875.89 KB |
| **EFCoreUpsert** |    **250000** | **34,470.08 ms** |    **NA** | **607000.0000** | **190000.0000** | **4292407.68 KB** |
|    BulkMerge |    250000 |  5,091.40 ms |    NA |  64000.0000 |   3000.0000 |  418668.28 KB |


[/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|                 Method | RowsCount |        Mean | Error |       Gen0 |      Gen1 |    Allocated |
|----------------------- |---------- |------------:|------:|-----------:|----------:|-------------:|
|    **ReturnDbGeneratedId** |       **100** |    **15.65 ms** |    **NA** |          **-** |         **-** |    **189.59 KB** |
| NotReturnDbGeneratedId |       100 |    20.96 ms |    NA |          - |         - |    148.23 KB |
|    **ReturnDbGeneratedId** |      **1000** |    **47.67 ms** |    **NA** |          **-** |         **-** |   **1707.91 KB** |
| NotReturnDbGeneratedId |      1000 |    49.77 ms |    NA |          - |         - |   1301.91 KB |
|    **ReturnDbGeneratedId** |     **10000** |   **234.40 ms** |    **NA** |  **2000.0000** | **1000.0000** |   **15867.1 KB** |
| NotReturnDbGeneratedId |     10000 |   441.70 ms |    NA |  1000.0000 |         - |  11897.58 KB |
|    **ReturnDbGeneratedId** |    **100000** | **2,330.98 ms** |    **NA** | **24000.0000** | **1000.0000** |  **157522.2 KB** |
| NotReturnDbGeneratedId |    100000 | 1,674.64 ms |    NA | 19000.0000 | 1000.0000 | 118806.84 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-FVGKFX : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|                 Method | RowsCount |     Mean | Error |        Gen0 |      Gen1 |  Allocated |
|----------------------- |---------- |---------:|------:|------------:|----------:|-----------:|
|    **ReturnDbGeneratedId** |    **250000** |  **5.101 s** |    **NA** |  **60000.0000** | **1000.0000** |  **381.14 MB** |
| NotReturnDbGeneratedId |    250000 |  4.681 s |    NA |  48000.0000 | 1000.0000 |  290.02 MB |
|    **ReturnDbGeneratedId** |    **500000** | **10.377 s** |    **NA** | **121000.0000** | **3000.0000** |  **763.73 MB** |
| NotReturnDbGeneratedId |    500000 |  9.393 s |    NA |  96000.0000 | 1000.0000 |  580.01 MB |
|    **ReturnDbGeneratedId** |   **1000000** | **21.818 s** |    **NA** | **243000.0000** | **6000.0000** | **1529.56 MB** |
| NotReturnDbGeneratedId |   1000000 | 22.181 s |    NA | 193000.0000 | 2000.0000 | 1160.27 MB |


### BulkMatch
Single Column [/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |          Mean | Error |        Gen0 |       Gen1 |      Gen2 |   Allocated |
|------------------ |---------- |--------------:|------:|------------:|-----------:|----------:|------------:|
|      **EFCoreSelect** |       **100** |    **107.695 ms** |    **NA** |           **-** |          **-** |         **-** |   **765.44 KB** |
| EFCoreBatchSelect |       100 |      3.477 ms |    NA |           - |          - |         - |    60.54 KB |
|         BulkMatch |       100 |      8.764 ms |    NA |           - |          - |         - |    85.45 KB |
|      **EFCoreSelect** |      **1000** |    **862.412 ms** |    **NA** |   **1000.0000** |          **-** |         **-** |  **7361.88 KB** |
| EFCoreBatchSelect |      1000 |      6.029 ms |    NA |           - |          - |         - |    511.2 KB |
|         BulkMatch |      1000 |     12.584 ms |    NA |           - |          - |         - |   740.02 KB |
|      **EFCoreSelect** |     **10000** |  **8,139.137 ms** |    **NA** |  **11000.0000** |  **1000.0000** |         **-** | **70483.66 KB** |
| EFCoreBatchSelect |     10000 |     34.499 ms |    NA |           - |          - |         - |  5127.73 KB |
|         BulkMatch |     10000 |     59.851 ms |    NA |   1000.0000 |          - |         - |  7522.84 KB |
|      **EFCoreSelect** |    **100000** | **74,961.618 ms** |    **NA** | **115000.0000** | **24000.0000** | **1000.0000** | **705096.2 KB** |
| EFCoreBatchSelect |    100000 |    526.386 ms |    NA |   7000.0000 |  2000.0000 |         - | 51371.59 KB |
|         BulkMatch |    100000 |    376.410 ms |    NA |  11000.0000 |  4000.0000 |         - | 71420.53 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-FVGKFX : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |       Mean | Error |        Gen0 |       Gen1 |      Gen2 | Allocated |
|------------------ |---------- |-----------:|------:|------------:|-----------:|----------:|----------:|
| **EFCoreBatchSelect** |    **250000** |   **912.3 ms** |    **NA** |  **16000.0000** |  **8000.0000** |         **-** | **112.49 MB** |
|         BulkMatch |    250000 | 1,010.1 ms |    NA |  29000.0000 |  9000.0000 | 1000.0000 |  173.6 MB |
| **EFCoreBatchSelect** |    **500000** | **1,516.4 ms** |    **NA** |  **34000.0000** | **17000.0000** | **1000.0000** | **224.89 MB** |
|         BulkMatch |    500000 | 2,572.8 ms |    NA |  57000.0000 | 17000.0000 | 1000.0000 | 347.35 MB |
| **EFCoreBatchSelect** |   **1000000** | **3,360.8 ms** |    **NA** |  **68000.0000** | **34000.0000** | **1000.0000** |  **449.7 MB** |
|         BulkMatch |   1000000 | 3,810.3 ms |    NA | 113000.0000 | 32000.0000 | 1000.0000 | 694.86 MB |


Multiple Columns [/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |    Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|-------------:|
| **EFCoreSelect** |       **100** |    **104.54 ms** |    **NA** |           **-** |          **-** |    **987.75 KB** |
|    BulkMatch |       100 |     11.92 ms |    NA |           - |          - |    154.95 KB |
| **EFCoreSelect** |      **1000** |  **1,129.42 ms** |    **NA** |   **1000.0000** |          **-** |    **9372.7 KB** |
|    BulkMatch |      1000 |     26.21 ms |    NA |           - |          - |   1342.45 KB |
| **EFCoreSelect** |     **10000** |  **8,849.80 ms** |    **NA** |  **14000.0000** |  **3000.0000** |  **91054.35 KB** |
|    BulkMatch |     10000 |    133.98 ms |    NA |   2000.0000 |  1000.0000 |   13648.6 KB |
| **EFCoreSelect** |    **100000** | **79,089.28 ms** |    **NA** | **148000.0000** | **38000.0000** | **910681.02 KB** |
|    BulkMatch |    100000 |    770.69 ms |    NA |  20000.0000 |  5000.0000 | 129760.33 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-FVGKFX : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|    Method | RowsCount |    Mean | Error |        Gen0 |       Gen1 |  Allocated |
|---------- |---------- |--------:|------:|------------:|-----------:|-----------:|
| **BulkMatch** |    **250000** | **3.090 s** |    **NA** |  **51000.0000** | **13000.0000** |  **318.85 MB** |
| **BulkMatch** |    **500000** | **4.753 s** |    **NA** | **104000.0000** | **27000.0000** |  **640.03 MB** |
| **BulkMatch** |   **1000000** | **9.823 s** |    **NA** | **209000.0000** | **54000.0000** | **1282.65 MB** |


### TempTable
[/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/TempTableBenchmarks.cs](/src/EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks/TempTableBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-SKLMHH : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|          Method | RowsCount |        Mean | Error |        Gen0 |      Gen1 |      Gen2 |    Allocated |
|---------------- |---------- |------------:|------:|------------:|----------:|----------:|-------------:|
| **CreateTempTable** |       **100** |    **11.01 ms** |    **NA** |           **-** |         **-** |         **-** |    **120.83 KB** |
| **CreateTempTable** |      **1000** |    **22.78 ms** |    **NA** |           **-** |         **-** |         **-** |    **732.55 KB** |
| **CreateTempTable** |     **10000** |   **106.20 ms** |    **NA** |   **1000.0000** | **1000.0000** |         **-** |   **6851.52 KB** |
| **CreateTempTable** |    **100000** |   **335.78 ms** |    **NA** |  **12000.0000** | **2000.0000** | **1000.0000** |  **68058.16 KB** |
| **CreateTempTable** |    **250000** |   **823.61 ms** |    **NA** |  **27000.0000** | **1000.0000** |         **-** | **170011.08 KB** |
| **CreateTempTable** |    **500000** | **1,586.69 ms** |    **NA** |  **55000.0000** | **1000.0000** |         **-** | **339932.95 KB** |
| **CreateTempTable** |   **1000000** | **3,280.07 ms** |    **NA** | **110000.0000** | **1000.0000** |         **-** |  **679776.7 KB** |


## License
**EntityFrameworkCore.PostgreSQL.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
