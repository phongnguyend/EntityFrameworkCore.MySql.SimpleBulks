using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace EntityFrameworkCore.MySql.SimpleBulks.DbContextExtensionsTests.Database;

public class TestDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _schema;
    private readonly bool _enableDiscriminator;

    public DbSet<SingleKeyRow<int>> SingleKeyRows { get; set; }

    public DbSet<CompositeKeyRow<int, int>> CompositeKeyRows { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public TestDbContext(string connectionString, string schema, bool enableDiscriminator)
    {
        _connectionString = connectionString;
        _schema = schema;
        _enableDiscriminator = enableDiscriminator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 39));

        if (!string.IsNullOrEmpty(_schema))
        {
            optionsBuilder.UseMySql(_connectionString, serverVersion, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}"));

            this.RegisterSchemaNameTranslator((schema, table) => $"{schema}_{table}");
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

        modelBuilder.Entity<SingleKeyRow<int>>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<CompositeKeyRow<int, int>>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<CompositeKeyRow<int, int>>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");

        modelBuilder.Entity<Customer>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<Contact>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        if (_enableDiscriminator)
        {
            modelBuilder.Entity<ExtendedSingleKeyRow<int>>();
            modelBuilder.Entity<ExtendedCompositeKeyRow<int, int>>();
            modelBuilder.Entity<ExtendedConfigurationEntry>();
        }

        base.OnModelCreating(modelBuilder);
    }
}
