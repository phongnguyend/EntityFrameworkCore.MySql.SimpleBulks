using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace EntityFrameworkCore.MySql.SimpleBulks.Demo.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
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
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
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
            name: "CompositeKeyRows");

        migrationBuilder.DropTable(
            name: "ConfigurationEntries");

        migrationBuilder.DropTable(
            name: "Rows");
    }
}
