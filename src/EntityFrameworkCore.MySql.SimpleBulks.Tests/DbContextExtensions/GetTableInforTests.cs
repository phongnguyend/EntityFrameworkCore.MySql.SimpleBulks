using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

public class GetTableInforTests
{
    protected string GetConnectionString(string dbPrefixName)
    {
        return $"server=localhost;database={dbPrefixName}.{Guid.NewGuid()};user=root;password=mysql;AllowLoadLocalInfile=true";
    }

    protected TestDbContext GetDbContext(string dbPrefixName, string schema)
    {
        return new TestDbContext(GetConnectionString(dbPrefixName), schema);
    }

    [Fact]
    public void GetTableInfor_ReturnsCorrectTableInformation()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Assert
        Assert.Equal("ConfigurationEntry", tableInfor.Name);
        Assert.Equal("`ConfigurationEntry`", tableInfor.SchemaQualifiedTableName);
    }

    [Fact]
    public void GetTableInfor_ReturnsFromCache()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var tableInfor1 = dbContext.GetTableInfor<ConfigurationEntry>();
        var tableInfor2 = dbContext.GetTableInfor<ConfigurationEntry>();

        // Assert
        Assert.Equal(tableInfor1, tableInfor2);
    }

    [Fact]
    public async Task GetTableInfor_MultiThreads_ShoudReturnFromCache()
    {
        // Arrange && Act
        var tasks = new List<Task<TableInfor<ConfigurationEntry>>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                using var dbct = GetDbContext("Tests", "");
                return dbct.GetTableInfor<ConfigurationEntry>();
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        var dbContext = GetDbContext("Tests", "");

        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        foreach (var task in tasks)
        {
            // Assert
            Assert.Equal(tableInfor, task.Result);
        }
    }
}
