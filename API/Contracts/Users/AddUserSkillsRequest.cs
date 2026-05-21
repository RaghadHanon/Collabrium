namespace API.Contracts.Users;

public sealed class AddUserSkillsRequest
{
    public List<string> SkillIds { get; set; } = new();
}