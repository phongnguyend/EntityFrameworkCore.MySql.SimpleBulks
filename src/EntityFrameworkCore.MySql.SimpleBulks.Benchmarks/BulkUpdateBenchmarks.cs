using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.MySql.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.MySql.SimpleBulks.BulkInsert;
using EntityFrameworkCore.MySql.SimpleBulks.BulkUpdate;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.MySql.SimpleBulks.Benchmarks;

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkUpdateBenchmarks1
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Guid> _customerIds;

    [Params(100, 1000, 10_000, 100_000, 250_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true");
        _context.Database.EnsureCreated();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

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

        var insertOptions = new BulkInsertOptions
        {
            Timeout = 0
        };

        _context.BulkInsert(_customers, insertOptions);

        _customerIds = _customers.Select(x => x.Id).ToList();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void EFCoreUpdate()
    {
        var pageSize = 10_000;
        var pages = _customerIds.Chunk(pageSize);

        var random = new Random(2024);

        foreach (var page in pages)
        {
            var customers = _context.Customers.Where(x => page.Contains(x.Id)).ToList();

            foreach (var customer in customers)
            {
                customer.FirstName = "Updated" + random.Next();
            }
        }

        _context.SaveChanges();
    }

    [Benchmark]
    public void BulkUpdate()
    {
        var random = new Random(2024);

        foreach (var customer in _customers)
        {
            customer.FirstName = "Updated" + random.Next();
        }

        var updateOptions = new BulkUpdateOptions
        {
            Timeout = 0
        };

        _context.BulkUpdate(_customers,
        x => new { x.FirstName },
        updateOptions);
    }
}

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkUpdateBenchmarks2
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Guid> _customerIds;

    [Params(500_000, 1_000_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true");
        _context.Database.EnsureCreated();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

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

        var insertOptions = new BulkInsertOptions
        {
            Timeout = 0
        };

        _context.BulkInsert(_customers, insertOptions);

        _customerIds = _customers.Select(x => x.Id).ToList();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void BulkUpdate()
    {
        var random = new Random(2024);

        foreach (var customer in _customers)
        {
            customer.FirstName = "Updated" + random.Next();
        }

        var updateOptions = new BulkUpdateOptions
        {
            Timeout = 0
        };

        _context.BulkUpdate(_customers,
        x => new { x.FirstName },
        updateOptions);
    }
}