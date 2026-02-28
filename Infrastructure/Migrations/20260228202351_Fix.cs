using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectHistories_AspNetUsers_ChangedByUserId",
                table: "ProjectHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProjectHistories_ChangedByUserId",
                table: "ProjectHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "ProjectHistories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedByUserId",
                table: "ProjectHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistories_ChangedByUserId",
                table: "ProjectHistories",
                column: "ChangedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectHistories_AspNetUsers_ChangedByUserId",
                table: "ProjectHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}