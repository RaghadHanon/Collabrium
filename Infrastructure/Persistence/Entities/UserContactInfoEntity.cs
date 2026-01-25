namespace Infrastructure.Persistence.Entities;

public class UserContactInfoEntity : ContactInfoEntity
{
    public string? Github { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public ApplicationUserEntity? User { get; set; }
}
