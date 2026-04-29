using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

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
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnedShippingAddress_Street = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnedShippingAddress_Location_Lat = table.Column<double>(type: "double", nullable: true),
                    OwnedShippingAddress_Location_Lng = table.Column<double>(type: "double", nullable: true),
                    ComplexShippingAddress_Street = table.Column<string>(type: "longtext", nullable: true),
                    ComplexShippingAddress_Location_Lat = table.Column<double>(type: "double", nullable: true),
                    ComplexShippingAddress_Location_Lng = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexOwnedTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShippingAddress_Street = table.Column<string>(type: "longtext", nullable: true),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "double", nullable: true),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompositeKeyRows",
                columns: table => new
                {
                    Id1 = table.Column<int>(type: "int", nullable: false),
                    Id2 = table.Column<int>(type: "int", nullable: false),
                    Column1 = table.Column<int>(type: "int", nullable: false),
                    Column2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Column3 = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfigurationEntries",
                columns: table => new
                {
                    Id1 = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Key1 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSensitive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SeasonAsInt = table.Column<int>(type: "int", nullable: true),
                    SeasonAsString = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationEntries", x => x.Id1);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JsonComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ComplexShippingAddress = table.Column<string>(type: "json", nullable: false),
                    OwnedShippingAddress = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexOwnedTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JsonComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShippingAddress = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JsonOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShippingAddress = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonOwnedTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShippingAddress_Street = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "double", nullable: true),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedTypeOrders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Column1 = table.Column<int>(type: "int", nullable: false),
                    Column2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Column3 = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Decimal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Double = table.Column<double>(type: "double", nullable: false),
                    Short = table.Column<short>(type: "smallint", nullable: false),
                    Long = table.Column<long>(type: "bigint", nullable: false),
                    Float = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rows", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
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
