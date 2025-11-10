using Npgsql;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public class ParameterInfo
{
    public string Name { get; set; }

    public string Type { get; set; }

    public  NpgsqlParameter Parameter { get; set; }
}
