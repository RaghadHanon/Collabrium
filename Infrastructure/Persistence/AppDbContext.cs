using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUserEntity>(options)
{
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<TeamMemberEntity> TeamMembers => Set<TeamMemberEntity>();

    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<ProjectHistoryEntity> ProjectHistories => Set<ProjectHistoryEntity>();

    public DbSet<SprintEntity> Sprints => Set<SprintEntity>();

    public DbSet<WorkItemEntity> WorkItems => Set<WorkItemEntity>();
    public DbSet<AttachmentEntity> Attachments => Set<AttachmentEntity>();
    public DbSet<CodeLinkEntity> CodeLinks => Set<CodeLinkEntity>();

    public DbSet<TaskItemEntity> Tasks => Set<TaskItemEntity>();
    public DbSet<TestCaseEntity> TestCases => Set<TestCaseEntity>();
    public DbSet<EpicEntity> Epics => Set<EpicEntity>();
    public DbSet<UserStoryEntity> UserStories => Set<UserStoryEntity>();
    public DbSet<BugEntity> Bugs => Set<BugEntity>();

    public DbSet<WorkItemRelationEntity> WorkItemRelations => Set<WorkItemRelationEntity>();
    public DbSet<CommentEntity> Comments => Set<CommentEntity>();

    public DbSet<RoadmapEntity> Roadmaps => Set<RoadmapEntity>();
    public DbSet<RoadmapNodeEntity> RoadmapNodes => Set<RoadmapNodeEntity>();
    public DbSet<TopicNodeEntity> TopicNodes => Set<TopicNodeEntity>();
    public DbSet<ReferenceNodeEntity> ReferenceNodes => Set<ReferenceNodeEntity>();
    public DbSet<SubTopicEntity> SubTopics => Set<SubTopicEntity>();
    public DbSet<ResourceEntity> Resources => Set<ResourceEntity>();
    public DbSet<CourseAssignmentEntity> CourseAssignments => Set<CourseAssignmentEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<CourseRatingEntity> CourseRatings => Set<CourseRatingEntity>();
    public DbSet<UserCourseEntity> UserCourses => Set<UserCourseEntity>();

    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<RoadmapTagEntity> RoadmapTags => Set<RoadmapTagEntity>(); // join table

    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        TimeConverter(builder);
    }

    private static void TimeConverter(ModelBuilder builder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime())
                : v,
            v => v.HasValue
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                : v
        );

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}