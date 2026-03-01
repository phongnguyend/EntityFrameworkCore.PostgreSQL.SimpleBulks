namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;

public class JsonComplexTypeOrder
{
    public int Id { get; set; }

    public ComplexTypeAddress ShippingAddress { get; set; }
}