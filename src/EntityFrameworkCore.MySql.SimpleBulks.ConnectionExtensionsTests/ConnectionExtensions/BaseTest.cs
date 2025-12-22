using EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit.Abstractions;

namespace EntityFrameworkCore.MySql.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly MySqlFixture _fixture;
    protected readonly TestDbContext _context;
    protected readonly MySqlConnection _connection;
    private string _schema = Environment.GetEnvironmentVariable("SCHEMA") ?? "";

    protected BaseTest(ITestOutputHelper output, MySqlFixture fixture, string dbPrefixName)
    {
        _output = output;
        _fixture = fixture;

        var connectionString = _fixture.GetConnectionString(dbPrefixName);

        _context = GetDbContext(connectionString);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
        _connection = new MySqlConnection(connectionString);

        TableMapper.Configure<SingleKeyRow<int>>(config =>
        {
            config
            .TableName(GetTableName("SingleKeyRows"))
            .PrimaryKeys(x => x.Id);
        });

        TableMapper.Configure<CompositeKeyRow<int, int>>(config =>
        {
            config
            .TableName(GetTableName("CompositeKeyRows"))
            .PrimaryKeys(x => new { x.Id1, x.Id2 });
        });

        TableMapper.Configure<ConfigurationEntry>(config =>
        {
            config
            .TableName(GetTableName("ConfigurationEntry"))
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ClientGenerated)
            .ReadOnlyProperty(x => x.RowVersion);
        });

        TableMapper.Configure<Customer>(config =>
        {
            config
            .TableName(GetTableName("Customers"))
            .IgnoreProperty(x => x.Contacts);
        });

        TableMapper.Configure<Contact>(config =>
        {
            config
            .TableName(GetTableName("Contacts"))
            .IgnoreProperty(x => x.Customer);
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string connectionString)
    {
        return new TestDbContext(connectionString, _schema);
    }

    protected string GetTableName(string tableName)
    {
        return string.IsNullOrEmpty(_schema) ? tableName : $"{_schema}_{tableName}";
    }

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }
}
