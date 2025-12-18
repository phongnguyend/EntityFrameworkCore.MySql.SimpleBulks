using EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly MySqlFixture _fixture;
    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, MySqlFixture fixture, string dbPrefixName)
    {
        _output = output;
        _fixture = fixture;
        _context = GetDbContext(dbPrefixName);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string dbPrefixName)
    {
        string schema = Environment.GetEnvironmentVariable("SCHEMA") ?? "";
        bool enableDiscriminator = (Environment.GetEnvironmentVariable("DISCRIMINATOR") ?? "") == "true";

        Console.WriteLine($"Schema: {schema}, Enable Discriminator: {enableDiscriminator}");

        return new TestDbContext(_fixture.GetConnectionString(dbPrefixName), schema, enableDiscriminator);
    }

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }
}
