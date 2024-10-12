using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "CompositeKeyRows",
                columns: table => new
                {
                    Id1 = table.Column<int>(type: "integer", nullable: false),
                    Id2 = table.Column<int>(type: "integer", nullable: false),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: false),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
                });

            migrationBuilder.CreateTable(
                name: "CompositeKeyRows",
                schema: "test",
                columns: table => new
                {
                    Id1 = table.Column<int>(type: "integer", nullable: false),
                    Id2 = table.Column<int>(type: "integer", nullable: false),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: false),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationEntry",
                columns: table => new
                {
                    Id1 = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Key1 = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsSensitive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationEntry", x => x.Id1);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    CurrentCountryIsoCode = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleKeyRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: false),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleKeyRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleKeyRows",
                schema: "test",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: false),
                    Column3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleKeyRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    CountryIsoCode = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CustomerId",
                table: "Contacts",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompositeKeyRows");

            migrationBuilder.DropTable(
                name: "CompositeKeyRows",
                schema: "test");

            migrationBuilder.DropTable(
                name: "ConfigurationEntry");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "SingleKeyRows");

            migrationBuilder.DropTable(
                name: "SingleKeyRows",
                schema: "test");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
