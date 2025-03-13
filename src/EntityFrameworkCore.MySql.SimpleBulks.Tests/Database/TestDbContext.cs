using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;

public class TestDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _schema;

    public DbSet<SingleKeyRow<int>> SingleKeyRows { get; set; }

    public DbSet<CompositeKeyRow<int, int>> CompositeKeyRows { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public TestDbContext(string connectionString, string schema)
    {
        _connectionString = connectionString;
        _schema = schema;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 39));

        if (!string.IsNullOrEmpty(_schema))
        {
            optionsBuilder.UseMySql(_connectionString, serverVersion, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}"));
        }
        else
        {
            optionsBuilder.UseMySql(_connectionString, serverVersion);
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrEmpty(_schema))
        {
            modelBuilder.HasDefaultSchema(_schema);
        }

        modelBuilder.Entity<CompositeKeyRow<int, int>>().HasKey(x => new { x.Id1, x.Id2 });

        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");

        base.OnModelCreating(modelBuilder);
    }
}
