using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPropertiesToProjectEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "EndDate",
            table: "Projects",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "StartDate",
            table: "Projects",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "Projects",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EndDate",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "StartDate",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "Projects");
    }
}
