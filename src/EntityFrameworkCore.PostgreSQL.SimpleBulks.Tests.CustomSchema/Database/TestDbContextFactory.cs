using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        return new TestDbContext("Server=.;Database=EFCoreSimpleBulksTests;Username=postgres;Password=postgres");
    }
}
