using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_workItems_WorkItemId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_workItems_IncomingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_workItems_OutgoingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_workItems_AspNetUsers_AssignedToUserId",
                table: "workItems");

            migrationBuilder.DropForeignKey(
                name: "FK_workItems_AspNetUsers_CreatedByUserId",
                table: "workItems");

            migrationBuilder.DropForeignKey(
                name: "FK_workItems_Projects_ProjectId",
                table: "workItems");

            migrationBuilder.DropForeignKey(
                name: "FK_workItems_Sprints_SprintId",
                table: "workItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workItems",
                table: "workItems");

            migrationBuilder.RenameTable(
                name: "workItems",
                newName: "WorkItem");

            migrationBuilder.RenameIndex(
                name: "IX_workItems_SprintId",
                table: "WorkItem",
                newName: "IX_WorkItem_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_workItems_ProjectId",
                table: "WorkItem",
                newName: "IX_WorkItem_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_workItems_CreatedByUserId",
                table: "WorkItem",
                newName: "IX_WorkItem_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_workItems_AssignedToUserId",
                table: "WorkItem",
                newName: "IX_WorkItem_AssignedToUserId");

            migrationBuilder.AddColumn<string>(
                name: "ActualResult",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bug_ActualResult",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bug_ExpectedResult",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "WorkItem",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedResult",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preconditions",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Steps",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StepsToReproduce",
                table: "WorkItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestEnvironment",
                table: "WorkItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkItem",
                table: "WorkItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_WorkItem_WorkItemId",
                table: "Comments",
                column: "WorkItemId",
                principalTable: "WorkItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItem_AspNetUsers_AssignedToUserId",
                table: "WorkItem",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItem_AspNetUsers_CreatedByUserId",
                table: "WorkItem",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItem_Projects_ProjectId",
                table: "WorkItem",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItem_Sprints_SprintId",
                table: "WorkItem",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_WorkItem_IncomingWorkItemId",
                table: "workItemRelations",
                column: "IncomingWorkItemId",
                principalTable: "WorkItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_WorkItem_OutgoingWorkItemId",
                table: "workItemRelations",
                column: "OutgoingWorkItemId",
                principalTable: "WorkItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_WorkItem_WorkItemId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItem_AspNetUsers_AssignedToUserId",
                table: "WorkItem");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItem_AspNetUsers_CreatedByUserId",
                table: "WorkItem");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItem_Projects_ProjectId",
                table: "WorkItem");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItem_Sprints_SprintId",
                table: "WorkItem");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_WorkItem_IncomingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_WorkItem_OutgoingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkItem",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "ActualResult",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "Bug_ActualResult",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "Bug_ExpectedResult",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "ExpectedResult",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "Preconditions",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "Steps",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "StepsToReproduce",
                table: "WorkItem");

            migrationBuilder.DropColumn(
                name: "TestEnvironment",
                table: "WorkItem");

            migrationBuilder.RenameTable(
                name: "WorkItem",
                newName: "workItems");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_SprintId",
                table: "workItems",
                newName: "IX_workItems_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_ProjectId",
                table: "workItems",
                newName: "IX_workItems_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_CreatedByUserId",
                table: "workItems",
                newName: "IX_workItems_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_AssignedToUserId",
                table: "workItems",
                newName: "IX_workItems_AssignedToUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workItems",
                table: "workItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_workItems_WorkItemId",
                table: "Comments",
                column: "WorkItemId",
                principalTable: "workItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_workItems_IncomingWorkItemId",
                table: "workItemRelations",
                column: "IncomingWorkItemId",
                principalTable: "workItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_workItems_OutgoingWorkItemId",
                table: "workItemRelations",
                column: "OutgoingWorkItemId",
                principalTable: "workItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workItems_AspNetUsers_AssignedToUserId",
                table: "workItems",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_workItems_AspNetUsers_CreatedByUserId",
                table: "workItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workItems_Projects_ProjectId",
                table: "workItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workItems_Sprints_SprintId",
                table: "workItems",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }
    }
}
