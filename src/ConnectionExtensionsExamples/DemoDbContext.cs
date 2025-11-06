using ConnectionExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace ConnectionExtensionsExamples;

public class DemoDbContext : DbContext
{
    private string _connectionString = ConnectionStrings.MySqlConnectionString;

    public DbSet<Row> Rows { get; set; }

    public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

    public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 39));
        optionsBuilder.UseMySql(_connectionString, serverVersion)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });

        base.OnModelCreating(modelBuilder);
    }
}
