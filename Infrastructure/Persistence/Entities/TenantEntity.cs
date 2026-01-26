namespace Infrastructure.Persistence.Entities;

public class TenantEntity : DbEntity
{
    public string Name { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public string? TenantAdminId { get; set; }
    public string? WebsiteUrl { get; set; }

    // ✅ Owned
    public ContactDetailsOwned ContactInfo { get; set; } = new();
    public StoredImageOwned? ProfileImage { get; set; }

    public ICollection<ApplicationUserEntity> Users { get; set; } = new List<ApplicationUserEntity>();
    public ICollection<TeamEntity> Teams { get; set; } = new List<TeamEntity>();
    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
}