using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities;

public class ApplicationUserEntity : IdentityUser
{
    public string? TenantId { get; set; }
    public TenantEntity? Tenant { get; set; }

    public string FullName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public Department? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool MustChangePassword { get; set; } = false;

    public DateOnly DateOfBirth { get; set; }

    [NotMapped]
    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }

    public ContactDetailsOwned ContactDetails { get; set; } = new();
    public StoredImageOwned? ProfileImage { get; set; }

    public ICollection<TeamMemberEntity> TeamMemberships { get; set; } = new List<TeamMemberEntity>();

    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();

    public ICollection<WorkItemEntity> AssignedWorkItems { get; set; } = new List<WorkItemEntity>();
    public ICollection<WorkItemEntity> CreatedWorkItems { get; set; } = new List<WorkItemEntity>();
    public ICollection<ProjectHistoryEntity> ProjectsHistories { get; set; } = new List<ProjectHistoryEntity>();
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
}