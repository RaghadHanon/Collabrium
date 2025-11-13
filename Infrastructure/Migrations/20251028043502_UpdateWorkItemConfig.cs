using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkItemConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameTable(
                name: "WorkItem",
                newName: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "Discriminator",
                table: "WorkItems",
                newName: "WorkItemType");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_SprintId",
                table: "WorkItems",
                newName: "IX_WorkItems_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_ProjectId",
                table: "WorkItems",
                newName: "IX_WorkItems_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_CreatedByUserId",
                table: "WorkItems",
                newName: "IX_WorkItems_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItem_AssignedToUserId",
                table: "WorkItems",
                newName: "IX_WorkItems_AssignedToUserId");

            migrationBuilder.AddColumn<int>(
                name: "StoryPoints",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_WorkItems_WorkItemId",
                table: "Comments",
                column: "WorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_WorkItems_IncomingWorkItemId",
                table: "workItemRelations",
                column: "IncomingWorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workItemRelations_WorkItems_OutgoingWorkItemId",
                table: "workItemRelations",
                column: "OutgoingWorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_AspNetUsers_AssignedToUserId",
                table: "WorkItems",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_AspNetUsers_CreatedByUserId",
                table: "WorkItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Sprints_SprintId",
                table: "WorkItems",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_WorkItems_WorkItemId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_WorkItems_IncomingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_workItemRelations_WorkItems_OutgoingWorkItemId",
                table: "workItemRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_AspNetUsers_AssignedToUserId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_AspNetUsers_CreatedByUserId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Sprints_SprintId",
                table: "WorkItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "StoryPoints",
                table: "WorkItems");

            migrationBuilder.RenameTable(
                name: "WorkItems",
                newName: "WorkItem");

            migrationBuilder.RenameColumn(
                name: "WorkItemType",
                table: "WorkItem",
                newName: "Discriminator");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_SprintId",
                table: "WorkItem",
                newName: "IX_WorkItem_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItem",
                newName: "IX_WorkItem_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_CreatedByUserId",
                table: "WorkItem",
                newName: "IX_WorkItem_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_AssignedToUserId",
                table: "WorkItem",
                newName: "IX_WorkItem_AssignedToUserId");

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
    }
}
