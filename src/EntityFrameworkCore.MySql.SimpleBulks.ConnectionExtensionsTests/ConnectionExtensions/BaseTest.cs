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
    private string _schema = "";

    protected BaseTest(ITestOutputHelper output, MySqlFixture fixture, string dbPrefixName, string schema = "")
    {
        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);

        _context = GetDbContext(connectionString, schema);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("SET GLOBAL local_infile = 1;");
        _connection = new MySqlConnection(connectionString);
        _schema = schema;

        TableMapper.Register(new MySqlTableInfor<SingleKeyRow<int>>(GetTableName("SingleKeyRows"))
        {
            PrimaryKeys = ["Id"],
        });

        TableMapper.Register(new MySqlTableInfor<CompositeKeyRow<int, int>>(GetTableName("CompositeKeyRows"))
        {
            PrimaryKeys = ["Id1", "Id2"],
        });

        TableMapper.Register(new MySqlTableInfor<ConfigurationEntry>(GetTableName("ConfigurationEntry"))
        {
            PrimaryKeys = ["Id"],
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ClientGenerated,
            }
        });

        TableMapper.Register(new MySqlTableInfor<Customer>(GetTableName("Customers"))
        {
            PropertyNames = ["Id", "FirstName", "LastName", "CurrentCountryIsoCode", "Index", "Season", "SeasonAsString"]
        });

        TableMapper.Register(new MySqlTableInfor<Contact>(GetTableName("Contacts"))
        {
            PropertyNames = ["Id", "EmailAddress", "PhoneNumber", "CountryIsoCode", "Index", "Season", "SeasonAsString", "CustomerId"]
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string connectionString, string schema)
    {
        return new TestDbContext(connectionString, schema);
    }

    protected string GetTableName(string tableName)
    {
        return string.IsNullOrEmpty(_schema) ? tableName : $"{_schema}_{tableName}";
    }
}
