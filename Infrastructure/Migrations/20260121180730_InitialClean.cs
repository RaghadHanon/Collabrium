using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialClean : Migration
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

        migrationBuilder.DropColumn(
            name: "ProfilePictureUrl",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "YearsOfExperience",
            table: "AspNetUsers");

        migrationBuilder.RenameTable(
            name: "workItems",
            newName: "WorkItems");

        migrationBuilder.RenameIndex(
            name: "IX_workItems_SprintId",
            table: "WorkItems",
            newName: "IX_WorkItems_SprintId");

        migrationBuilder.RenameIndex(
            name: "IX_workItems_ProjectId",
            table: "WorkItems",
            newName: "IX_WorkItems_ProjectId");

        migrationBuilder.RenameIndex(
            name: "IX_workItems_CreatedByUserId",
            table: "WorkItems",
            newName: "IX_WorkItems_CreatedByUserId");

        migrationBuilder.RenameIndex(
            name: "IX_workItems_AssignedToUserId",
            table: "WorkItems",
            newName: "IX_WorkItems_AssignedToUserId");

        migrationBuilder.AddColumn<string>(
            name: "ActualResult",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Bug_ActualResult",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Bug_ExpectedResult",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExpectedResult",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Preconditions",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Steps",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StepsToReproduce",
            table: "WorkItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "StoryPoints",
            table: "WorkItems",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TestEnvironment",
            table: "WorkItems",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WorkItemType",
            table: "WorkItems",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TenantAdminId",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WebsiteURL",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TenantId",
            table: "AspNetUsers",
            type: "nvarchar(450)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AlterColumn<int>(
            name: "Department",
            table: "AspNetUsers",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            table: "AspNetUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddPrimaryKey(
            name: "PK_WorkItems",
            table: "WorkItems",
            column: "Id");

        migrationBuilder.CreateTable(
            name: "Images",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                OwnerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Images", x => x.Id);
                table.ForeignKey(
                    name: "FK_Images_AspNetUsers_OwnerId",
                    column: x => x.OwnerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Images_Tenants_OwnerId",
                    column: x => x.OwnerId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TenantContactInfos",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                LinkedIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                YouTube = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantContactInfos", x => x.Id);
                table.ForeignKey(
                    name: "FK_TenantContactInfos_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserContactInfos",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Github = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                LinkedIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                YouTube = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserContactInfos", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserContactInfos_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_Name",
            table: "Tenants",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Images_OwnerId",
            table: "Images",
            column: "OwnerId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TenantContactInfos_TenantId",
            table: "TenantContactInfos",
            column: "TenantId",
            unique: true,
            filter: "[TenantId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_UserContactInfos_UserId",
            table: "UserContactInfos",
            column: "UserId",
            unique: true,
            filter: "[UserId] IS NOT NULL");

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

        migrationBuilder.DropTable(
            name: "Images");

        migrationBuilder.DropTable(
            name: "TenantContactInfos");

        migrationBuilder.DropTable(
            name: "UserContactInfos");

        migrationBuilder.DropPrimaryKey(
            name: "PK_WorkItems",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_Tenant_Name",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "ActualResult",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Bug_ActualResult",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Bug_ExpectedResult",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "ExpectedResult",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Preconditions",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Steps",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "StepsToReproduce",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "StoryPoints",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "TestEnvironment",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "WorkItemType",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "Country",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "TenantAdminId",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "WebsiteURL",
            table: "Tenants");

        migrationBuilder.DropColumn(
            name: "MustChangePassword",
            table: "AspNetUsers");

        migrationBuilder.RenameTable(
            name: "WorkItems",
            newName: "workItems");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItems_SprintId",
            table: "workItems",
            newName: "IX_workItems_SprintId");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItems_ProjectId",
            table: "workItems",
            newName: "IX_workItems_ProjectId");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItems_CreatedByUserId",
            table: "workItems",
            newName: "IX_workItems_CreatedByUserId");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItems_AssignedToUserId",
            table: "workItems",
            newName: "IX_workItems_AssignedToUserId");

        migrationBuilder.AlterColumn<string>(
            name: "TenantId",
            table: "AspNetUsers",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Department",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProfilePictureUrl",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "YearsOfExperience",
            table: "AspNetUsers",
            type: "int",
            nullable: false,
            defaultValue: 0);

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
