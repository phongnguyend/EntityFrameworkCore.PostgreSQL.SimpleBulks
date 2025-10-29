using Npgsql;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public record struct ConnectionContext(NpgsqlConnection Connection, NpgsqlTransaction Transaction);