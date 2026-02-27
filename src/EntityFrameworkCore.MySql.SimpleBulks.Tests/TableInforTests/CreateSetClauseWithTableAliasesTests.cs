using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.TableInforTests;

public class CreateSetClauseWithTableAliasesTests
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
    public void CreateSetClause_WithTableAliases_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", "t", "s", null);

        // Assert
        Assert.Equal("t.`Value` = s.`Value`", result);
    }

    [Fact]
    public void CreateSetClause_WithTableAliases_AndColumnMapping_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act - Key is mapped to Key1 column
        var result = tableInfor.CreateSetClause("Key", "t", "s", null);

        // Assert
        Assert.Equal("t.`Key1` = s.`Key`", result);
    }

    [Fact]
    public void CreateSetClause_WithTableAliases_AndCustomConfigureSetStatement_ReturnsCustomStatement()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", "t", "s", ctx =>
        {
            return ctx.Left + " = COALESCE(" + ctx.Right + ", " + ctx.TargetTableAlias + ".`Value`)";
        });

        // Assert
        Assert.Equal("t.`Value` = COALESCE(s.`Value`, t.`Value`)", result);
    }

    [Fact]
    public void CreateSetClause_WithTableAliases_AndCustomConfigureSetStatementReturnsNull_ReturnsDefaultStatement()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", "t", "s", ctx => null);

        // Assert
        Assert.Equal("t.`Value` = s.`Value`", result);
    }

    [Fact]
    public void CreateSetClause_WithTableAliases_ConfigureSetStatementReceivesCorrectContext()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();
        SetClauseContext? capturedContext = null;

        // Act
        tableInfor.CreateSetClause("Value", "target", "source", ctx =>
        {
            capturedContext = ctx;
            return null;
        });

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Equal(tableInfor, capturedContext.Value.TableInfor);
        Assert.Equal("Value", capturedContext.Value.PropertyName);
        Assert.Equal("target.`Value`", capturedContext.Value.Left);
        Assert.Equal("source.`Value`", capturedContext.Value.Right);
        Assert.Equal("target", capturedContext.Value.TargetTableAlias);
        Assert.Equal("source", capturedContext.Value.SourceTableAlias);
    }

    [Fact]
    public void CreateSetClause_WithTableAliases_PropertyWithDot_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = GetDbContext("Tests", "");
        var tableInfor = dbContext.GetTableInfor<OwnedTypeOrder>();

        // Act
        var result = tableInfor.CreateSetClause("ShippingAddress.Street", "t", "s", null);

        // Assert
        Assert.Equal("t.`ShippingAddress_Street` = s.`ShippingAddress.Street`", result);
    }
}
