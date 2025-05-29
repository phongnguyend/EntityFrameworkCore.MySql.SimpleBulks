using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

public class GetPropertiesTests
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
    public void GetProperties_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var properties = dbContext.GetProperties(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(8, properties.Count);

        var idProperty = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(Guid), idProperty.PropertyType);
        Assert.Equal("Id1", idProperty.ColumnName);
        Assert.Equal("char(36)", idProperty.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
        Assert.Null(idProperty.DefaultValueSql);
        Assert.True(idProperty.IsPrimaryKey);
        Assert.False(idProperty.IsRowVersion);


        var versionProperty = properties.First(p => p.PropertyName == "RowVersion");
        Assert.Equal(typeof(byte[]), versionProperty.PropertyType);
        Assert.Equal("RowVersion", versionProperty.ColumnName);
        Assert.Equal("timestamp(6)", versionProperty.ColumnType);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, versionProperty.ValueGenerated);
        Assert.Null(versionProperty.DefaultValueSql);
        Assert.False(versionProperty.IsPrimaryKey);
        Assert.True(versionProperty.IsRowVersion);
    }

    [Fact]
    public void GetProperties_ShouldReturnFromCache()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var properties1 = dbContext.GetProperties(typeof(ConfigurationEntry));
        var properties2 = dbContext.GetProperties(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(properties1, properties2);
    }
}
