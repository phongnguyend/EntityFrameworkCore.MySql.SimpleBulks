using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.MySqlConnectionExtensions;

public abstract class BaseTest: IDisposable
{
    protected readonly ITestOutputHelper _output;

    protected readonly TestDbContext _context;
    protected readonly MySqlConnection _connection;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        _output = output;

        var connectionString = GetConnectionString(dbPrefixName);

        _context = GetDbContext(connectionString);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
        _connection = new MySqlConnection(connectionString);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"server=localhost;database={dbPrefixName}.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true";
    }

    protected TestDbContext GetDbContext(string connectionString)
    {
        return new TestDbContext(connectionString);
    }
}
