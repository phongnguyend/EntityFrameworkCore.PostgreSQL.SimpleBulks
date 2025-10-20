using Npgsql;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;

public static class NpgsqlConnectionExtensions
{
    public static void EnsureOpen(this NpgsqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    public static async Task EnsureOpenAsync(this NpgsqlConnection connection, CancellationToken cancellationToken = default)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    public static void EnsureClosed(this NpgsqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public static NpgsqlCommand CreateTextCommand(this NpgsqlConnection connection, NpgsqlTransaction transaction, string commandText, BulkOptions options = null)
    {
        options ??= new BulkOptions()
        {
            BatchSize = 0,
            Timeout = 30,
        };

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.CommandTimeout = options.Timeout;
        return command;
    }
}
