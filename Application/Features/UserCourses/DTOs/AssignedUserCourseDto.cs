using Domain.Enums;

namespace Application.Features.UserCourses.DTOs;

public sealed class AssignedUserCourseDto
{
    public required string UserCourseId { get; set; }

    public required string UserId { get; set; }
    public string? UserName { get; set; }

    public required string CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public string? CourseThumbnailUrl { get; set; }
    public string? AssignedByUserId { get; set; }
    public string? AssignedByUserName { get; set; }
    public bool IsAssigned { get; set; }
    public DateTime? AssignedAt { get; set; }

    public UserCourseStatus Status { get; set; }
    public bool IsSaved { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
