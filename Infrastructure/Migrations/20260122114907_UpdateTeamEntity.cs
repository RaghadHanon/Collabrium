using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateTeamEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Capacity",
            table: "Teams",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Department",
            table: "Teams",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Teams",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "TeamLeadId",
            table: "Teams",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Teams",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Teams_TeamLeadId",
            table: "Teams",
            column: "TeamLeadId");

        migrationBuilder.AddForeignKey(
            name: "FK_Teams_AspNetUsers_TeamLeadId",
            table: "Teams",
            column: "TeamLeadId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Teams_AspNetUsers_TeamLeadId",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_TeamLeadId",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "Capacity",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "Department",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "TeamLeadId",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Teams");
    }
}
