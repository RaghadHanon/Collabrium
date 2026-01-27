using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<TaskItemEntity> Tasks => Set<TaskItemEntity>();
    public DbSet<TestCaseEntity> TestCases => Set<TestCaseEntity>();
    public DbSet<EpicEntity> Epics => Set<EpicEntity>();
    public DbSet<UserStoryEntity> UserStories => Set<UserStoryEntity>();
    public DbSet<BugEntity> Bugs => Set<BugEntity>();

    public DbSet<WorkItemRelationEntity> WorkItemRelations => Set<WorkItemRelationEntity>();
    public DbSet<CommentEntity> Comments => Set<CommentEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}