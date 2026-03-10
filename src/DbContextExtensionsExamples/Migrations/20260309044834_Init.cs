using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DbContextExtensionsExamples.Migrations
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
                name: "ComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnedShippingAddress_Street = table.Column<string>(type: "text", nullable: true),
                    OwnedShippingAddress_Location_Lat = table.Column<double>(type: "double precision", nullable: true),
                    OwnedShippingAddress_Location_Lng = table.Column<double>(type: "double precision", nullable: true),
                    ComplexShippingAddress_Street = table.Column<string>(type: "text", nullable: true),
                    ComplexShippingAddress_Location_Lat = table.Column<double>(type: "double precision", nullable: true),
                    ComplexShippingAddress_Location_Lng = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShippingAddress_Street = table.Column<string>(type: "text", nullable: true),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "double precision", nullable: true),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexTypeOrders", x => x.Id);
                });

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
                    Id1 = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Key1 = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSensitive = table.Column<bool>(type: "boolean", nullable: false),
                    SeasonAsInt = table.Column<int>(type: "integer", nullable: true),
                    SeasonAsString = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationEntries", x => x.Id1);
                });

            migrationBuilder.CreateTable(
                name: "JsonComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComplexShippingAddress = table.Column<string>(type: "jsonb", nullable: false),
                    OwnedShippingAddress = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JsonComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShippingAddress = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JsonOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShippingAddress = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShippingAddress_Street = table.Column<string>(type: "text", nullable: true),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "double precision", nullable: true),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedTypeOrders", x => x.Id);
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
                name: "ComplexOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "ComplexTypeOrders");

            migrationBuilder.DropTable(
                name: "CompositeKeyRows");

            migrationBuilder.DropTable(
                name: "ConfigurationEntries");

            migrationBuilder.DropTable(
                name: "JsonComplexOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "JsonComplexTypeOrders");

            migrationBuilder.DropTable(
                name: "JsonOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "OwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "Rows");
        }
    }
}
