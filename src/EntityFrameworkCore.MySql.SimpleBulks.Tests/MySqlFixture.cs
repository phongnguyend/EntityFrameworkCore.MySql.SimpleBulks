using MySqlConnector;
using Testcontainers.MySql;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests;

public class MySqlFixture : IAsyncLifetime
{
    private bool UseContainer => true;

    public MySqlContainer? Container { get; }

    public MySqlFixture()
    {
        if (!UseContainer)
        {
            return;
        }

        Container = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.DisposeAsync();
    }

    public string GetConnectionString(string dbPrefixName)
    {
        if (!UseContainer)
        {
            return $"server=localhost;database={dbPrefixName}.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true";
        }

        var connectionStringBuilder = new MySqlConnectionStringBuilder(Container!.GetConnectionString());
        connectionStringBuilder.Database = $"{dbPrefixName}.{Guid.NewGuid()}";
        connectionStringBuilder.UserID = "root";
        connectionStringBuilder.AllowLoadLocalInfile = true;

        return connectionStringBuilder.ToString();
    }
}

