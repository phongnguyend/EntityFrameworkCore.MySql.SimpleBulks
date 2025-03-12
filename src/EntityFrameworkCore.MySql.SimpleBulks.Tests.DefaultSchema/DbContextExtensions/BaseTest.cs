using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

public abstract class BaseTest: IDisposable
{
    protected readonly ITestOutputHelper _output;

    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        _output = output;
        _context = GetDbContext(dbPrefixName);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"server=localhost;database={dbPrefixName}.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true";
    }

    protected TestDbContext GetDbContext(string dbPrefixName)
    {
        return new TestDbContext(GetConnectionString(dbPrefixName));
    }
}
