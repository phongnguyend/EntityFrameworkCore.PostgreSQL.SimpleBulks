using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.TypeMapperTests;

public class GenerateTempTableTests
{
    private readonly TestDbContext _dbContext;

    public GenerateTempTableTests()
    {
        _dbContext = new TestDbContext("", "");
    }

    [Fact]
    public Task ToDataTable_SingleKeyRow()
    {
        var rows = new List<SingleKeyRow<int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Spring
            });
        }

        var properties = new[]
        {
            "Id",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(SingleKeyRow<int>));

        var script = TypeMapper.GenerateTempTableDefinition<SingleKeyRow<int>>("SingleKeyRows", properties, null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_CompositeKeyRow()
    {
        var rows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Spring
            });
        }

        var properties = new[]
        {
            "Id1",
            "Id2",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(CompositeKeyRow<int, int>));

        var script = TypeMapper.GenerateTempTableDefinition<CompositeKeyRow<int, int>>("CompositeKeyRows", properties, null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_ColumnMapping()
    {
        var rows = new List<ConfigurationEntry>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var properties = new[]
        {
            "Id",
            "Key",
            "Value",
            "Description",
            "CreatedDateTime"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(ConfigurationEntry));
        var columnNames = _dbContext.GetColumnNames(typeof(ConfigurationEntry));
        var columnTypes = _dbContext.GetColumnTypes(typeof(ConfigurationEntry));

        var script = TypeMapper.GenerateTempTableDefinition<ConfigurationEntry>("ConfigurationEntries", properties, columnNames, columnTypes);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_ComplexType()
    {
        var orders = new List<ComplexTypeOrder>
        {
            new() {},
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Street = "123 Main St"
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Location = new ComplexTypeLocation
                    {

                    }
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128,
                        Lng = -74.0060
                    }
                }
            }
        };

        var properties = new[]
        {
            "Id",
            "ShippingAddress.Street",
            "ShippingAddress.Location.Lat",
            "ShippingAddress.Location.Lng"
        };


        var script = TypeMapper.GenerateTempTableDefinition<ComplexTypeOrder>("ComplexTypeOrders", properties, null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_OwnedType()
    {
        var orders = new List<OwnedTypeOrder>
        {
            new() {},
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Street = "123 Main St"
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Location = new OwnedTypeLocation
                    {

                    }
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128,
                        Lng = -74.0060
                    }
                }
            }
        };

        var properties = new[]
        {
            "Id",
            "ShippingAddress.Street",
            "ShippingAddress.Location.Lat",
            "ShippingAddress.Location.Lng"
        };

        var script = TypeMapper.GenerateTempTableDefinition<OwnedTypeOrder>("OwnedTypeOrders", properties, null, null);

        // Assert
        return Verify(script);
    }
}
