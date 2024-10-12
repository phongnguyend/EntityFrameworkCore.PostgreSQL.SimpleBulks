using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks.Database;

internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        return new TestDbContext("Server=.;Database=SimpleBulks.Benchmarks;Username=postgres;Password=postgres");
    }
}
