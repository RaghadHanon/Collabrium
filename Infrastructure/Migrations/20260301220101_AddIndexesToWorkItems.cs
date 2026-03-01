using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_AssignedToUserId",
                table: "WorkItems",
                columns: new[] { "ProjectId", "AssignedToUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_CreatedAt",
                table: "WorkItems",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_Priority",
                table: "WorkItems",
                columns: new[] { "ProjectId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_SprintId_CreatedAt",
                table: "WorkItems",
                columns: new[] { "ProjectId", "SprintId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_Status",
                table: "WorkItems",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId_Type",
                table: "WorkItems",
                columns: new[] { "ProjectId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_AssignedToUserId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_CreatedAt",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_Priority",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_SprintId_CreatedAt",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_Status",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId_Type",
                table: "WorkItems");
        }
    }
}
