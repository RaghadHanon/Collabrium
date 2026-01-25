namespace Infrastructure.Persistence.Entities;

public class UserImageEntity : ImageEntity
{
    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUserEntity ApplicationUser { get; set; } = null!;
}
