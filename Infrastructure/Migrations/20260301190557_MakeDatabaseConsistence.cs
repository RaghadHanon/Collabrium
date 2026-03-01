using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeDatabaseConsistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
              name: "JobTitle",
              table: "AspNetUsers",
              type: "nvarchar(MAX)",
              nullable: true,
              oldClrType: typeof(int),
              oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
              name: "JobTitle",
              table: "AspNetUsers",
              type: "int",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(MAX)");
        }
    }
}
