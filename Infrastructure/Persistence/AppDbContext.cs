using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUserEntity>(options)
{
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<SprintEntity> Sprints => Set<SprintEntity>();
    public DbSet<WorkItemEntity> WorkItems => Set<WorkItemEntity>();
    public DbSet<TaskItemEntity> Tasks => Set<TaskItemEntity>();
    public DbSet<TestCaseEntity> TestCases => Set<TestCaseEntity>();
    public DbSet<EpicEntity> Epics => Set<EpicEntity>();
    public DbSet<UserStoryEntity> UserStores => Set<UserStoryEntity>();
    public DbSet<BugEntity> Bugs => Set<BugEntity>();
    public DbSet<WorkItemRelationEntity> workItemRelations => Set<WorkItemRelationEntity>();
    public DbSet<CommentEntity> Comments => Set<CommentEntity>();
    public DbSet<TenantImageEntity> TenantImages => Set<TenantImageEntity>();
    public DbSet<UserImageEntity> UserImages => Set<UserImageEntity>();
    public DbSet<TenantContactInfoEntity> TenantContactInfos => Set<TenantContactInfoEntity>();
    public DbSet<UserContactInfoEntity> UserContactInfos => Set<UserContactInfoEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Automatically apply all IEntityTypeConfiguration<T> classes
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}