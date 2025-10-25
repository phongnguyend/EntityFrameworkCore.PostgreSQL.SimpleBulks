using EntityFrameworkCore.PostgreSQL.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Demo;

public class DemoDbContext : DbContext
{
    private const string _connectionString = "Host=127.0.0.1;Database=EFCoreSimpleBulks;Username=postgres;Password=postgres";

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
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        base.OnModelCreating(modelBuilder);
    }
}
