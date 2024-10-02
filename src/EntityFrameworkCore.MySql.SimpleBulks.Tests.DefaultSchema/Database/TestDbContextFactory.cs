using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        return new TestDbContext("Server=.;Database=EFCoreSimpleBulksTests;user=root;password=mysql;AllowLoadLocalInfile=true");
    }
}
