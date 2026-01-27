using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateTenantEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TenantAdminId",
            table: "Tenants",
            type: "nvarchar(450)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_TenantAdminId",
            table: "Tenants",
            column: "TenantAdminId");

        migrationBuilder.AddForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants",
            column: "TenantAdminId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants");

        migrationBuilder.DropIndex(
            name: "IX_Tenants_TenantAdminId",
            table: "Tenants");

        migrationBuilder.AlterColumn<string>(
            name: "TenantAdminId",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldNullable: true);
    }
}
