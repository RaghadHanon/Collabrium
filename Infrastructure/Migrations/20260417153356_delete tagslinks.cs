using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deletetagslinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapTags_Roadmaps_RoadmapId",
                table: "RoadmapTags");

            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapTags_Tags_TagId",
                table: "RoadmapTags");

            migrationBuilder.DropTable(
                name: "CourseTagEntity");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "RoadmapTags",
                newName: "TagsId");

            migrationBuilder.RenameColumn(
                name: "RoadmapId",
                table: "RoadmapTags",
                newName: "RoadmapEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_RoadmapTag_TagId",
                table: "RoadmapTags",
                newName: "IX_RoadmapTags_TagsId");

            migrationBuilder.CreateTable(
                name: "CourseTags",
                columns: table => new
                {
                    CourseEntityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TagsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTags", x => new { x.CourseEntityId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_CourseTags_Courses_CourseEntityId",
                        column: x => x.CourseEntityId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTags_TagsId",
                table: "CourseTags",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapTags_Roadmaps_RoadmapEntityId",
                table: "RoadmapTags",
                column: "RoadmapEntityId",
                principalTable: "Roadmaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapTags_Tags_TagsId",
                table: "RoadmapTags",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapTags_Roadmaps_RoadmapEntityId",
                table: "RoadmapTags");

            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapTags_Tags_TagsId",
                table: "RoadmapTags");

            migrationBuilder.DropTable(
                name: "CourseTags");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "RoadmapTags",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "RoadmapEntityId",
                table: "RoadmapTags",
                newName: "RoadmapId");

            migrationBuilder.RenameIndex(
                name: "IX_RoadmapTags_TagsId",
                table: "RoadmapTags",
                newName: "IX_RoadmapTag_TagId");

            migrationBuilder.CreateTable(
                name: "CourseTagEntity",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TagId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTagEntity", x => new { x.CourseId, x.TagId });
                    table.ForeignKey(
                        name: "FK_CourseTagEntity_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTagEntity_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTags_TagId",
                table: "CourseTagEntity",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapTags_Roadmaps_RoadmapId",
                table: "RoadmapTags",
                column: "RoadmapId",
                principalTable: "Roadmaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapTags_Tags_TagId",
                table: "RoadmapTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
