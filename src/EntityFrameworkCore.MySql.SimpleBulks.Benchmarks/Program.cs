using BenchmarkDotNet.Running;
using EntityFrameworkCore.MySql.SimpleBulks.Benchmarks;


//var test = new BulkMergeBenchmarks();
//test.RowsCount = 250_000;
//test.IterationSetup();
//test.BulkMerge();
//test.IterationCleanup();
//return;

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