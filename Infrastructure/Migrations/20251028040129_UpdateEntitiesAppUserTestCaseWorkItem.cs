using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateEntitiesAppUserTestCaseWorkItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProfilePictureUrl",
            table: "AspNetUsers");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "workItems",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Image",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Image", x => x.Id);
                table.ForeignKey(
                    name: "FK_Image_AspNetUsers_ApplicationUserId",
                    column: x => x.ApplicationUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Image_ApplicationUserId",
            table: "Image",
            column: "ApplicationUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Image");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "workItems");

        migrationBuilder.AddColumn<string>(
            name: "ProfilePictureUrl",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true);
    }
}
