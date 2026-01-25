namespace Domain.Models;

public sealed class UserContactInfo : ContactInfo
{
    public string? Github { get; private set; } = string.Empty;

    public void UpdateGithub(string? github)
    {
        Github = github ?? string.Empty;
    }
}
