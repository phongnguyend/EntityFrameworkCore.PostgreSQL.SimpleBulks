using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConnectionExtensionsExamples.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "CompositeKeyRows",
                columns: table => new
                {
                    Id1 = table.Column<int>(type: "integer", nullable: false),
                    Id2 = table.Column<int>(type: "integer", nullable: false),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSensitive = table.Column<bool>(type: "boolean", nullable: false),
                    SeasonAsInt = table.Column<int>(type: "integer", nullable: true),
                    SeasonAsString = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rows", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompositeKeyRows");

            migrationBuilder.DropTable(
                name: "ConfigurationEntries");

            migrationBuilder.DropTable(
                name: "Rows");
        }
    }
}
