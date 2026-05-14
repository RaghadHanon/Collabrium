using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class inital : Migration
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
                name: "ArticleComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleLikes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLikes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CoverImagePublicId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTags",
                columns: table => new
                {
                    ArticleEntityId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TagsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTags", x => new { x.ArticleEntityId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ArticleTags_Articles_ArticleEntityId",
                        column: x => x.ArticleEntityId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                ContactDetails_Socials_Github = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
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
                });

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_RefreshTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
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
                TenantAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                table.ForeignKey(
                    name: "FK_Tenants_AspNetUsers_TenantAdminId",
                    column: x => x.TenantAdminId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Provider = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                InstructorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Level = table.Column<int>(type: "int", nullable: false),
                Language = table.Column<int>(type: "int", nullable: false),
                EstimatedDurationInHours = table.Column<int>(type: "int", nullable: true),
                ThumbnailImage_Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                ThumbnailImage_PublicId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
                table.ForeignKey(
                    name: "FK_Courses_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Roadmaps",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Path = table.Column<int>(type: "int", nullable: false),
                Level = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roadmaps", x => x.Id);
                table.ForeignKey(
                    name: "FK_Roadmaps_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Roadmaps_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tags_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
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
            name: "CourseLearningPoints",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CourseLearningPoints", x => x.Id);
                table.ForeignKey(
                    name: "FK_CourseLearningPoints_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CourseRatings",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Rating = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CourseRatings", x => x.Id);
                table.ForeignKey(
                    name: "FK_CourseRatings_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CourseRatings_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CourseRatings_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserCourses",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                IsSaved = table.Column<bool>(type: "bit", nullable: false),
                IsAssigned = table.Column<bool>(type: "bit", nullable: false),
                AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserCourses", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserCourses_AspNetUsers_AssignedByUserId",
                    column: x => x.AssignedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_UserCourses_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_UserCourses_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserCourses_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RoadmapNodes",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoadmapId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false),
                NodeType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                RefRoadmapId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapNodes", x => x.Id);
                table.ForeignKey(
                    name: "FK_RoadmapNodes_Roadmaps_RefRoadmapId",
                    column: x => x.RefRoadmapId,
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RoadmapNodes_Roadmaps_RoadmapId",
                    column: x => x.RoadmapId,
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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

        migrationBuilder.CreateTable(
            name: "RoadmapTags",
            columns: table => new
            {
                RoadmapEntityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TagsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapTags", x => new { x.RoadmapEntityId, x.TagsId });
                table.ForeignKey(
                    name: "FK_RoadmapTags_Roadmaps_RoadmapEntityId",
                    column: x => x.RoadmapEntityId,
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RoadmapTags_Tags_TagsId",
                    column: x => x.TagsId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastWorkItemNumber = table.Column<int>(type: "int", nullable: false),
                LastSprintNumber = table.Column<int>(type: "int", nullable: false),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TeamId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
            name: "SubTopics",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TopicNodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubTopics", x => x.Id);
                table.ForeignKey(
                    name: "FK_SubTopics_RoadmapNodes_TopicNodeId",
                    column: x => x.TopicNodeId,
                    principalTable: "RoadmapNodes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectHistories",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ActionType = table.Column<int>(type: "int", nullable: false),
                Scope = table.Column<int>(type: "int", nullable: false),
                RelatedEntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RelatedEntityTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RelatedEntityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MadeByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectHistories_AspNetUsers_MadeByUserId",
                    column: x => x.MadeByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
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
                TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Goal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
            name: "Resources",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SubTopicId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Resources", x => x.Id);
                table.ForeignKey(
                    name: "FK_Resources_SubTopics_SubTopicId",
                    column: x => x.SubTopicId,
                    principalTable: "SubTopics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkItems",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                AcceptanceCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                EffortPoints = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                Priority = table.Column<int>(type: "int", nullable: false),
                IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SprintId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                ParentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                StepsToReproduce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BugEntity_ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BugEntity_ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Preconditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Steps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TestEnvironment = table.Column<int>(type: "int", nullable: true),
                ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Result = table.Column<int>(type: "int", nullable: true),
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
                table.ForeignKey(
                    name: "FK_WorkItems_WorkItems_ParentId",
                    column: x => x.ParentId,
                    principalTable: "WorkItems",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                WorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Attachments_AspNetUsers_UploadedByUserId",
                    column: x => x.UploadedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
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
                Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                LinkType = table.Column<int>(type: "int", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CodeLinks", x => x.Id);
                table.ForeignKey(
                    name: "FK_CodeLinks_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CodeLinks_WorkItems_WorkItemId",
                    column: x => x.WorkItemId,
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkItemComments",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                WorkItemId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItemComments", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItemComments_Comments_Id",
                    column: x => x.Id,
                    principalTable: "Comments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkItemComments_WorkItems_WorkItemId",
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
                    onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleComments_ArticleId",
                table: "ArticleComments",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLikes_ArticleId_CreatedAt",
                table: "ArticleLikes",
                columns: new[] { "ArticleId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLikes_ArticleId_CreatedByUserId",
                table: "ArticleLikes",
                columns: new[] { "ArticleId", "CreatedByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLikes_CreatedByUserId",
                table: "ArticleLikes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreatedByUserId",
                table: "Articles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TenantId_CreatedAt",
                table: "Articles",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TenantId_IsPublished",
                table: "Articles",
                columns: new[] { "TenantId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TenantId_Title",
                table: "Articles",
                columns: new[] { "TenantId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTags_TagsId",
                table: "ArticleTags",
                column: "TagsId");

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
            name: "IX_Attachments_UploadedByUserId",
            table: "Attachments",
            column: "UploadedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_WorkItemId",
            table: "Attachments",
            column: "WorkItemId");

        migrationBuilder.CreateIndex(
            name: "IX_CodeLinks_CreatedByUserId",
            table: "CodeLinks",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_CodeLinks_WorkItemId",
            table: "CodeLinks",
            column: "WorkItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_CreatedByUserId",
            table: "Comments",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_CourseLearningPoints_CourseId",
            table: "CourseLearningPoints",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_CourseRatings_CourseId",
            table: "CourseRatings",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_CourseRatings_TenantId_UserId_CourseId",
            table: "CourseRatings",
            columns: new[] { "TenantId", "UserId", "CourseId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CourseRatings_UserId",
            table: "CourseRatings",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Courses_TenantId_Title",
            table: "Courses",
            columns: new[] { "TenantId", "Title" });

        migrationBuilder.CreateIndex(
            name: "IX_Courses_Title",
            table: "Courses",
            column: "Title");

        migrationBuilder.CreateIndex(
            name: "IX_CourseTags_TagsId",
            table: "CourseTags",
            column: "TagsId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHistories_MadeByUserId",
            table: "ProjectHistories",
            column: "MadeByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHistories_ProjectId_CreatedAt",
            table: "ProjectHistories",
            columns: new[] { "ProjectId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_CreatedByUserId",
            table: "Projects",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_Key",
            table: "Projects",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TeamId",
            table: "Projects",
            column: "TeamId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TenantId",
            table: "Projects",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_Token",
            table: "RefreshTokens",
            column: "Token",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId",
            table: "RefreshTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Resource_SubTopic_Order",
            table: "Resources",
            columns: new[] { "SubTopicId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ReferenceNode_RefRoadmapId",
            table: "RoadmapNodes",
            column: "RefRoadmapId");

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapNodes_RoadmapId_Order",
            table: "RoadmapNodes",
            columns: new[] { "RoadmapId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Roadmap_Title",
            table: "Roadmaps",
            column: "Title");

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_CreatedByUserId",
            table: "Roadmaps",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_TenantId",
            table: "Roadmaps",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapTags_TagsId",
            table: "RoadmapTags",
            column: "TagsId");

        migrationBuilder.CreateIndex(
            name: "IX_Sprints_Code",
            table: "Sprints",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Sprints_ProjectId",
            table: "Sprints",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_SubTopic_TopicNode_Order",
            table: "SubTopics",
            columns: new[] { "TopicNodeId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tag_Tenant_Name",
            table: "Tags",
            columns: new[] { "TenantId", "Name" },
            unique: true);

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
            name: "IX_Tenants_TenantAdminId",
            table: "Tenants",
            column: "TenantAdminId");

        migrationBuilder.CreateIndex(
            name: "IX_UserCourses_AssignedByUserId",
            table: "UserCourses",
            column: "AssignedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserCourses_CourseId",
            table: "UserCourses",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_UserCourses_TenantId_UserId_CourseId",
            table: "UserCourses",
            columns: new[] { "TenantId", "UserId", "CourseId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserCourses_UserId",
            table: "UserCourses",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemComments_WorkItemId",
            table: "WorkItemComments",
            column: "WorkItemId");

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
            name: "IX_WorkItems_Code",
            table: "WorkItems",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_CreatedByUserId",
            table: "WorkItems",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ParentId",
            table: "WorkItems",
            column: "ParentId");

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

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_SprintId",
            table: "WorkItems",
            column: "SprintId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleComments_Articles_ArticleId",
                table: "ArticleComments",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleComments_Comments_Id",
                table: "ArticleComments",
                column: "Id",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleLikes_Articles_ArticleId",
                table: "ArticleLikes",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleLikes_AspNetUsers_CreatedByUserId",
                table: "ArticleLikes",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AspNetUsers_CreatedByUserId",
                table: "Articles",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Tenants_TenantId",
                table: "Articles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleTags_Tags_TagsId",
                table: "ArticleTags",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            table: "AspNetUserLogins",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            table: "AspNetUserRoles",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUsers_Tenants_TenantId",
            table: "AspNetUsers",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tenants_AspNetUsers_TenantAdminId",
            table: "Tenants");

            migrationBuilder.DropTable(
                name: "ArticleComments");

            migrationBuilder.DropTable(
                name: "ArticleLikes");

            migrationBuilder.DropTable(
                name: "ArticleTags");

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
            name: "Attachments");

        migrationBuilder.DropTable(
            name: "CodeLinks");

        migrationBuilder.DropTable(
            name: "CourseLearningPoints");

        migrationBuilder.DropTable(
            name: "CourseRatings");

        migrationBuilder.DropTable(
            name: "CourseTags");

        migrationBuilder.DropTable(
            name: "ProjectHistories");

        migrationBuilder.DropTable(
            name: "RefreshTokens");

        migrationBuilder.DropTable(
            name: "Resources");

        migrationBuilder.DropTable(
            name: "RoadmapTags");

        migrationBuilder.DropTable(
            name: "TeamMembers");

        migrationBuilder.DropTable(
            name: "UserCourses");

        migrationBuilder.DropTable(
            name: "WorkItemComments");

        migrationBuilder.DropTable(
            name: "WorkItemRelations");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "SubTopics");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "Courses");

        migrationBuilder.DropTable(
            name: "Comments");

        migrationBuilder.DropTable(
            name: "WorkItems");

        migrationBuilder.DropTable(
            name: "RoadmapNodes");

        migrationBuilder.DropTable(
            name: "Sprints");

        migrationBuilder.DropTable(
            name: "Roadmaps");

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
