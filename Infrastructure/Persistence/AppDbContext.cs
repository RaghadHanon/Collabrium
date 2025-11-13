using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Domain.Entities.TaskItem> Tasks => Set<Domain.Entities.TaskItem>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<UserStory> UserStores => Set<UserStory>();
    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<WorkItemRelation> workItemRelations => Set<WorkItemRelation>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Automatically apply all IEntityTypeConfiguration<T> classes
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}