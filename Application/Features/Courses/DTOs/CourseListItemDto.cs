using Domain.Enums;

namespace Application.Features.Courses.DTOs;

public sealed class CourseListItemDto : CourseDto
{
    public bool IsSaved { get; set; }
    public UserCourseStatus? Status { get; set; }
    public bool IsAssignedToMe { get; set; }
}
