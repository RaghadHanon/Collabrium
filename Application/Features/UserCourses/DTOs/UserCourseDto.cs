using Application.Features.Courses.DTOs;
using Domain.Enums;

namespace Application.Features.UserCourses.DTOs
{
    public class UserCourseDto : CourseDto
    {
        public required string TenantId { get; set; }
        public required string UserId { get; set; }
        public required string CourseId { get; set; }

        public UserCourseStatus Status { get; set; }

        public bool IsSaved { get; set; }

        public bool IsAssigned { get; set; }
        public string? AssignedByUserId { get; set; }
        public string? AssignedByUserName { get; set; }
        public DateTime? AssignedAt { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
