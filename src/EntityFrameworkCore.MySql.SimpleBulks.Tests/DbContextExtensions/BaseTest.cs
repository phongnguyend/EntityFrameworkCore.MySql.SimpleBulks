using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly MySqlFixture _fixture;
    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, MySqlFixture fixture, string dbPrefixName, string schema = "")
    {
        _output = output;
        _fixture = fixture;
        _context = GetDbContext(dbPrefixName, schema);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string dbPrefixName, string schema)
    {
        return new TestDbContext(_fixture.GetConnectionString(dbPrefixName), schema);
    }
}
