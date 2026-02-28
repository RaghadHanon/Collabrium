using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RefactorCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Domain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                TenantAdminId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ContactDetails_Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                ContactDetails_PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ContactDetails_Socials_LinkedIn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Facebook = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Instagram = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_YouTube = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Github = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ProfileImage_Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                ProfileImage_PublicId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Department = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                ContactDetails_Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                ContactDetails_PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ContactDetails_Socials_LinkedIn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Facebook = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Instagram = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_YouTube = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                ContactDetails_Socials_Github = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ProfileImage_Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                ProfileImage_PublicId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUsers_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Teams",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TeamLeadId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Department = table.Column<int>(type: "int", nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Teams", x => x.Id);
                table.ForeignKey(
                    name: "FK_Teams_AspNetUsers_TeamLeadId",
                    column: x => x.TeamLeadId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Teams_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
                table.ForeignKey(
                    name: "FK_Projects_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Projects_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Projects_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TeamMembers",
            columns: table => new
            {
                TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TeamMembers", x => new { x.TeamId, x.UserId });
                table.ForeignKey(
                    name: "FK_TeamMembers_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TeamMembers_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectHistories",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ChangedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectHistories_AspNetUsers_ChangedByUserId",
                    column: x => x.ChangedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProjectHistories_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Sprints",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sprints", x => x.Id);
                table.ForeignKey(
                    name: "FK_Sprints_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkItems",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                AcceptanceCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                EffortPoints = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                Priority = table.Column<int>(type: "int", nullable: false),
                IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SprintId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                StepsToReproduce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BugEntity_ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BugEntity_ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Preconditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Steps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TestEnvironment = table.Column<int>(type: "int", nullable: true),
                ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                StoryPoints = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItems_AspNetUsers_AssignedToUserId",
                    column: x => x.AssignedToUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_WorkItems_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkItems_Sprints_SprintId",
                    column: x => x.SprintId,
                    principalTable: "Sprints",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                WorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comments_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comments_WorkItems_WorkItemId",
                    column: x => x.WorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkItemRelations",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SourceWorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TargetWorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RelationType = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItemRelations", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItemRelations_WorkItems_SourceWorkItemId",
                    column: x => x.SourceWorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItemRelations_WorkItems_TargetWorkItemId",
                    column: x => x.TargetWorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationUser_UserName",
            table: "AspNetUsers",
            column: "UserName",
            unique: true,
            filter: "[UserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_TenantId",
            table: "AspNetUsers",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_CreatedByUserId",
            table: "Comments",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_WorkItemId",
            table: "Comments",
            column: "WorkItemId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHistories_ChangedByUserId",
            table: "ProjectHistories",
            column: "MadeByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHistories_ProjectId",
            table: "ProjectHistories",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_CreatedByUserId",
            table: "Projects",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TeamId",
            table: "Projects",
            column: "TeamId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TenantId",
            table: "Projects",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Sprints_ProjectId",
            table: "Sprints",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_UserId",
            table: "TeamMembers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Team_Name",
            table: "Teams",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Teams_TeamLeadId",
            table: "Teams",
            column: "TeamLeadId");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_TenantId",
            table: "Teams",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_Domain",
            table: "Tenants",
            column: "Domain",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_Name",
            table: "Tenants",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemRelations_TargetWorkItemId",
            table: "WorkItemRelations",
            column: "TargetWorkItemId");

        migrationBuilder.CreateIndex(
            name: "UX_WorkItemRelation_Unique",
            table: "WorkItemRelations",
            columns: new[] { "SourceWorkItemId", "TargetWorkItemId", "RelationType" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_AssignedToUserId",
            table: "WorkItems",
            column: "AssignedToUserId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_CreatedByUserId",
            table: "WorkItems",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ProjectId",
            table: "WorkItems",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_SprintId",
            table: "WorkItems",
            column: "SprintId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "Comments");

        migrationBuilder.DropTable(
            name: "ProjectHistories");

        migrationBuilder.DropTable(
            name: "TeamMembers");

        migrationBuilder.DropTable(
            name: "WorkItemRelations");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "WorkItems");

        migrationBuilder.DropTable(
            name: "Sprints");

        migrationBuilder.DropTable(
            name: "Projects");

        migrationBuilder.DropTable(
            name: "Teams");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Tenants");
    }
}
