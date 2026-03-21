using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class makethoorderunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubTopic_TopicNode_Order",
                table: "SubTopics");

            migrationBuilder.DropIndex(
                name: "IX_RoadmapNodes_RoadmapId",
                table: "RoadmapNodes");

            migrationBuilder.DropIndex(
                name: "IX_Resource_SubTopic_Order",
                table: "Resources");

            migrationBuilder.CreateIndex(
                name: "IX_SubTopic_TopicNode_Order",
                table: "SubTopics",
                columns: new[] { "TopicNodeId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapNodes_RoadmapId_Order",
                table: "RoadmapNodes",
                columns: new[] { "RoadmapId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resource_SubTopic_Order",
                table: "Resources",
                columns: new[] { "SubTopicId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubTopic_TopicNode_Order",
                table: "SubTopics");

            migrationBuilder.DropIndex(
                name: "IX_RoadmapNodes_RoadmapId_Order",
                table: "RoadmapNodes");

            migrationBuilder.DropIndex(
                name: "IX_Resource_SubTopic_Order",
                table: "Resources");

            migrationBuilder.CreateIndex(
                name: "IX_SubTopic_TopicNode_Order",
                table: "SubTopics",
                columns: new[] { "TopicNodeId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapNodes_RoadmapId",
                table: "RoadmapNodes",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_SubTopic_Order",
                table: "Resources",
                columns: new[] { "SubTopicId", "Order" });
        }
    }
}
