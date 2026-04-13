namespace Infrastructure.Persistence.Entities;

public class UserSkillsTagEntity
{
    public string UserId { get; set; } = null!;
    public ApplicationUserEntity User { get; set; } = null!;

    public string TagId { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
