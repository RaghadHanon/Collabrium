using Application.Common.DTOs;
using Domain.Core;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Features.Users.DTOs;
public class UserDto
{
    public required string Id { get; set; }
    public string FullName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Department Department { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }
    public required ContactDetailsDto ContactDetails { get; set; }
}
