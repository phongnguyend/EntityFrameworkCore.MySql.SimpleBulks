﻿using BenchmarkDotNet.Running;
using EntityFrameworkCore.MySql.SimpleBulks.Benchmarks;
using System.Diagnostics;


var test = new BulkInsertSingleTableBenchmarks();
test.RowsCount = 100000;
test.IterationSetup();
var start = Stopwatch.GetTimestamp();
test.BulkInsert();
var elapsed = Stopwatch.GetElapsedTime(start);
Console.WriteLine($"Elapsed: {elapsed}");
test.IterationCleanup();
return;

_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();
_ = BenchmarkRunner.Run<BulkInsertMultipleTablesBenchmarks>();
_ = BenchmarkRunner.Run<BulkUpdateBenchmarks1>();
_ = BenchmarkRunner.Run<BulkUpdateBenchmarks2>();
_ = BenchmarkRunner.Run<BulkDeleteBenchmarks1>();
_ = BenchmarkRunner.Run<BulkDeleteBenchmarks2>();
_ = BenchmarkRunner.Run<BulkMergeBenchmarks>();
_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks1>();
_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks2>();
_ = BenchmarkRunner.Run<BulkMatchMultipleColumnsBenchmarks1>();
_ = BenchmarkRunner.Run<BulkMatchMultipleColumnsBenchmarks2>();
_ = BenchmarkRunner.Run<TempTableBenchmarks>();