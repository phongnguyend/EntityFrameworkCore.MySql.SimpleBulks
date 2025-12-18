using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.DbContextExtensions;

public class GetDiscriminatorTests
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
    public void GetDiscriminator_ReturnsNull_WhenNoDiscriminator()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(ConfigurationEntry));
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Assert
        Assert.Null(discriminator);
        Assert.Null(tableInfor.Discriminator);
    }

    [Fact]
    public void GetDiscriminator_ReturnsDiscriminator_Blog()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(Blog));
        var tableInfor = dbContext.GetTableInfor<Blog>();

        // Assert
        Assert.NotNull(discriminator);
        Assert.NotNull(tableInfor.Discriminator);

        Assert.Equal("Discriminator", discriminator.PropertyName);
        Assert.Equal(typeof(string), discriminator.PropertyType);
        Assert.Equal("Blog", discriminator.PropertyValue);
        Assert.Equal("Discriminator", discriminator.ColumnName);
        Assert.Equal("varchar(8)", discriminator.ColumnType);

        Assert.Equal(discriminator, tableInfor.Discriminator);
    }

    [Fact]
    public void GetDiscriminator_ReturnsDiscriminator_RssBlog()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(RssBlog));
        var tableInfor = dbContext.GetTableInfor<RssBlog>();

        // Assert
        Assert.NotNull(discriminator);
        Assert.NotNull(tableInfor.Discriminator);

        Assert.Equal("Discriminator", discriminator.PropertyName);
        Assert.Equal(typeof(string), discriminator.PropertyType);
        Assert.Equal("RssBlog", discriminator.PropertyValue);
        Assert.Equal("Discriminator", discriminator.ColumnName);
        Assert.Equal("varchar(8)", discriminator.ColumnType);

        Assert.Equal(discriminator, tableInfor.Discriminator);
    }
}
