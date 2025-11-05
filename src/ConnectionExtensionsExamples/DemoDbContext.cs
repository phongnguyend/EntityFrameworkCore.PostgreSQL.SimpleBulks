using ConnectionExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConnectionExtensionsExamples;

public class DemoDbContext : DbContext
{
    private string _connectionString;

    public DemoDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Row> Rows { get; set; }

    public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

    public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDefaultValueSql("uuid_generate_v1mc()");

        base.OnModelCreating(modelBuilder);
    }
}
