namespace Application.Features.Users.DTOs;

public class UserSnapshotDto
{
    public required string Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public required string JobTitle { get; set; }
}