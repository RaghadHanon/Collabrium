namespace API.Contracts.Users;

public sealed class DeleteUserSkillsRequest
{
    public List<string> SkillIds { get; set; } = new();
}