using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateTheRelationsOfTheTenants : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants");

        migrationBuilder.AddForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants",
            column: "TenantAdminId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants");

        migrationBuilder.AddForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants",
            column: "TenantAdminId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }
}
