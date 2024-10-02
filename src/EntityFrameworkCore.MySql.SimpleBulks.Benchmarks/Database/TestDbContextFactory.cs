using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.MySql.SimpleBulks.Benchmarks.Database;

internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        return new TestDbContext("server=localhost;database=EntityFrameworkCore.MySql.SimpleBulks.Benchmarks;user=root;password=mysql;AllowLoadLocalInfile=true");
    }
}
