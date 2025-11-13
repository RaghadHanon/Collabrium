using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UseEnumInWorkItemConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "WorkItemType",
            table: "WorkItems",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(13)",
            oldMaxLength: 13);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "WorkItemType",
            table: "WorkItems",
            type: "nvarchar(13)",
            maxLength: 13,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");
    }
}
