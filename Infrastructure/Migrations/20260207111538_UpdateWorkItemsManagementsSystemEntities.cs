using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateWorkItemsManagementsSystemEntities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UpdatedDate",
            table: "WorkItems");

        migrationBuilder.AddColumn<int>(
            name: "Result",
            table: "WorkItems",
            type: "int",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                WorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                UploadedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Attachments_WorkItems_WorkItemId",
                    column: x => x.WorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CodeLinks",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                WorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LinkType = table.Column<int>(type: "int", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CodeLinks", x => x.Id);
                table.ForeignKey(
                    name: "FK_CodeLinks_WorkItems_WorkItemId",
                    column: x => x.WorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_WorkItemId",
            table: "Attachments",
            column: "WorkItemId");

        migrationBuilder.CreateIndex(
            name: "IX_CodeLinks_WorkItemId",
            table: "CodeLinks",
            column: "WorkItemId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Attachments");

        migrationBuilder.DropTable(
            name: "CodeLinks");

        migrationBuilder.DropColumn(
            name: "Result",
            table: "WorkItems");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedDate",
            table: "WorkItems",
            type: "datetime2",
            nullable: true);
    }
}
