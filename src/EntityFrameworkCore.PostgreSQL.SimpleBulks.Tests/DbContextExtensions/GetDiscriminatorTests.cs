using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.DbContextExtensions;

public class GetDiscriminatorTests
{
    [Fact]
    public void GetDiscriminator_ReturnsNull_WhenNoDiscriminator()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(ConfigurationEntry));
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Assert
        Assert.Null(discriminator);
        Assert.Null(tableInfor.Discriminator);
    }

    [Fact]
    public void GetDiscriminator_ReturnsDiscriminator_Blog()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(Blog));
        var tableInfor = dbContext.GetTableInfor<Blog>();

        // Assert
        Assert.NotNull(discriminator);
        Assert.NotNull(tableInfor.Discriminator);

        Assert.Equal("Discriminator", discriminator.PropertyName);
        Assert.Equal(typeof(string), discriminator.PropertyType);
        Assert.Equal("Blog", discriminator.PropertyValue);
        Assert.Equal("Discriminator", discriminator.ColumnName);
        Assert.Equal("character varying(8)", discriminator.ColumnType);

        Assert.Equal(discriminator, tableInfor.Discriminator);
    }

    [Fact]
    public void GetDiscriminator_ReturnsDiscriminator_RssBlog()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var discriminator = dbContext.GetDiscriminator(typeof(RssBlog));
        var tableInfor = dbContext.GetTableInfor<RssBlog>();

        // Assert
        Assert.NotNull(discriminator);
        Assert.NotNull(tableInfor.Discriminator);

        Assert.Equal("Discriminator", discriminator.PropertyName);
        Assert.Equal(typeof(string), discriminator.PropertyType);
        Assert.Equal("RssBlog", discriminator.PropertyValue);
        Assert.Equal("Discriminator", discriminator.ColumnName);
        Assert.Equal("character varying(8)", discriminator.ColumnType);

        Assert.Equal(discriminator, tableInfor.Discriminator);
    }
}
