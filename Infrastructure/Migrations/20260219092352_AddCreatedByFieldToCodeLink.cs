using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByFieldToCodeLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItemRelations_WorkItems_TargetWorkItemId",
                table: "WorkItemRelations");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "CodeLinks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CodeLinks_CreatedByUserId",
                table: "CodeLinks",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeLinks_AspNetUsers_CreatedByUserId",
                table: "CodeLinks",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItemRelations_WorkItems_TargetWorkItemId",
                table: "WorkItemRelations",
                column: "TargetWorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodeLinks_AspNetUsers_CreatedByUserId",
                table: "CodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItemRelations_WorkItems_TargetWorkItemId",
                table: "WorkItemRelations");

            migrationBuilder.DropIndex(
                name: "IX_CodeLinks_CreatedByUserId",
                table: "CodeLinks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CodeLinks");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItemRelations_WorkItems_TargetWorkItemId",
                table: "WorkItemRelations",
                column: "TargetWorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
