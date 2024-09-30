using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkDelete;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.BulkInsert;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.CustomSchema;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Abstractions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Tests.NpgsqlConnectionExtensions
{
    public class BulkDeleteTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;
        private NpgsqlConnection _connection;

        public BulkDeleteTests(ITestOutputHelper output)
        {
            _output = output;

            var connectionString = $"Host=127.0.0.1;Database=BulkInsertTest.{Guid.NewGuid()};Username=postgres;Password=postgres";
            _context = new TestDbContext(connectionString);
            _context.Database.EnsureCreated();

            _connection = new NpgsqlConnection(connectionString);

            TableMapper.Register(typeof(SingleKeyRow<int>), TestConstants.Schema, "SingleKeyRows");
            TableMapper.Register(typeof(CompositeKeyRow<int, int>), TestConstants.Schema, "CompositeKeyRows");

            var rows = new List<SingleKeyRow<int>>();
            var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

            for (int i = 0; i < 100; i++)
            {
                rows.Add(new SingleKeyRow<int>
                {
                    Column1 = i,
                    Column2 = "" + i,
                    Column3 = DateTime.Now
                });

                compositeKeyRows.Add(new CompositeKeyRow<int, int>
                {
                    Id1 = i,
                    Id2 = i,
                    Column1 = i,
                    Column2 = "" + i,
                    Column3 = DateTime.Now
                });
            }

            _context.BulkInsert(rows,
                    row => new { row.Column1, row.Column2, row.Column3 });

            _context.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
        {
            var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
            var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

            if (useLinq)
            {
                if (omitTableName)
                {
                    _connection.BulkDelete(rows, row => row.Id,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                    _connection.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                }
                else
                {
                    _connection.BulkDelete(rows, new TableInfor(TestConstants.Schema, "SingleKeyRows"), row => row.Id,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                    _connection.BulkDelete(compositeKeyRows, new TableInfor(TestConstants.Schema, "CompositeKeyRows"), row => new { row.Id1, row.Id2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                }
            }
            else
            {
                if (omitTableName)
                {
                    _connection.BulkDelete(rows, "Id",
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                    _connection.BulkDelete(compositeKeyRows, ["Id1", "Id2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                }
                else
                {
                    _connection.BulkDelete(rows, new TableInfor(TestConstants.Schema, "SingleKeyRows"), "Id",
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                    _connection.BulkDelete(compositeKeyRows, new TableInfor(TestConstants.Schema, "CompositeKeyRows"), ["Id1", "Id2"],
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
                }
            }

            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            Assert.Single(dbRows);
            Assert.Single(dbCompositeKeyRows);
        }
    }
}