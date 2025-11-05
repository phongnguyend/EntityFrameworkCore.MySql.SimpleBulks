using DbContextExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace DbContextExtensionsExamples;

public class DemoDbContext : DbContext
{
    private const string _connectionString = "server=localhost;database=DbContextExtensionsExamples;user=root;password=mysql;AllowLoadLocalInfile=true";

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
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        base.OnModelCreating(modelBuilder);
    }
}
