using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class h : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    PublishAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Announcements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementAudiences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnnouncementId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementAudiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementAudiences_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementAudiences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnnouncementAudiences_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAudiences_AnnouncementId_Type",
                table: "AnnouncementAudiences",
                columns: new[] { "AnnouncementId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAudiences_RoleName",
                table: "AnnouncementAudiences",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAudiences_TeamId",
                table: "AnnouncementAudiences",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAudiences_UserId",
                table: "AnnouncementAudiences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedByUserId",
                table: "Announcements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TenantId_ExpiresAt",
                table: "Announcements",
                columns: new[] { "TenantId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TenantId_Priority",
                table: "Announcements",
                columns: new[] { "TenantId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TenantId_PublishAt",
                table: "Announcements",
                columns: new[] { "TenantId", "PublishAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementAudiences");

            migrationBuilder.DropTable(
                name: "Announcements");
        }
    }
}
