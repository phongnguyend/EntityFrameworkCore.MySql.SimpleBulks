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
        var tableInfor = dbContext.GetTableInfor(typeof(ConfigurationEntry));

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
        var tableInfor1 = dbContext.GetTableInfor(typeof(ConfigurationEntry));
        var tableInfor2 = dbContext.GetTableInfor(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(tableInfor1, tableInfor2);
    }
}
