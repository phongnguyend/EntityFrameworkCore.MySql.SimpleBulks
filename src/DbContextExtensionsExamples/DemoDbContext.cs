using DbContextExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;

namespace DbContextExtensionsExamples;

public class DemoDbContext : DbContext
{
    private string _connectionString = ConnectionStrings.MySqlConnectionString;

    public DbSet<Row> Rows { get; set; }

    public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

    public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

    public DbSet<ComplexTypeOrder> ComplexTypeOrders { get; set; }

    public DbSet<OwnedTypeOrder> OwnedTypeOrders { get; set; }

    public DbSet<ComplexOwnedTypeOrder> ComplexOwnedTypeOrders { get; set; }

    public DbSet<JsonComplexTypeOrder> JsonComplexTypeOrders { get; set; }

    public DbSet<JsonOwnedTypeOrder> JsonOwnedTypeOrders { get; set; }

    public DbSet<JsonComplexOwnedTypeOrder> JsonComplexOwnedTypeOrders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 39));
        optionsBuilder.UseMySql(_connectionString, serverVersion)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

#if NET9_0_OR_GREATER
        optionsBuilder.ConfigureWarnings(x => x.Log([RelationalEventId.PendingModelChangesWarning]));
#endif

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<JsonComplexTypeOrder>().ComplexProperty(x => x.ShippingAddress, x =>
        {
            x.ToJson();
            //x.ToJson("xxx").HasColumnType("json");
            x.ComplexProperty(y => y.Location, y =>
            {
                y.HasJsonPropertyName("xxx");
            });
        });

        modelBuilder.Entity<JsonOwnedTypeOrder>().OwnsOne(x => x.ShippingAddress, x =>
        {
            x.ToJson();
            //x.ToJson("xxx").HasColumnType("json");
            x.OwnsOne(y => y.Location, y =>
            {
                y.HasJsonPropertyName("xxx");
            });
        });

        modelBuilder.Entity<JsonComplexOwnedTypeOrder>().ComplexProperty(x => x.ComplexShippingAddress, x =>
        {
            x.ToJson();
            //x.ToJson("xxx").HasColumnType("json");
            x.ComplexProperty(y => y.Location, y =>
            {
                y.HasJsonPropertyName("xxx");
            });
        });

        modelBuilder.Entity<JsonComplexOwnedTypeOrder>().OwnsOne(x => x.OwnedShippingAddress, x =>
        {
            x.ToJson();
            //x.ToJson("xxx").HasColumnType("json");
            x.OwnsOne(y => y.Location, y =>
            {
                y.HasJsonPropertyName("xxx");
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}
