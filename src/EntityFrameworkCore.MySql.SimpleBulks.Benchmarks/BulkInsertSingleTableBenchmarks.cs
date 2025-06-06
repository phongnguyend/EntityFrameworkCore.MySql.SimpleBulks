﻿using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.MySql.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;

namespace EntityFrameworkCore.MySql.SimpleBulks.Benchmarks;

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkInsertSingleTableBenchmarks
{
    private TestDbContext _context;
    private List<Customer> _customers;

    [Params(100, 1000, 10_000, 100_000, 250_000, 500_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true");
        _context.Database.EnsureCreated();

        _customers = new List<Customer>(RowsCount);

        for (int i = 0; i < RowsCount; i++)
        {
            var customer = new Customer
            {
                FirstName = "FirstName " + i,
                LastName = "LastName " + i,
                Index = i,
            };
            _customers.Add(customer);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void EFCoreInsert()
    {
        _context.AddRange(_customers);
        _context.SaveChanges();
    }

    [Benchmark]
    public void BulkInsert()
    {
        _context.BulkInsert(_customers, opt =>
        {
            opt.Timeout = 0;
        });
    }
}
