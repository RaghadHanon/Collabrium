using Domain.Enums;

namespace API.Contracts.UserCourses
{
    public sealed class GetAssignedToMeCoursesRequest
    {
        public string? Search { get; set; }

        public UserCourseStatus? Status { get; set; }
        public bool? IsSaved { get; set; }

        public DateTime? AssignedAfter { get; set; }
        public DateTime? AssignedBefore { get; set; }

        public DateTime? StartedAfter { get; set; }
        public DateTime? StartedBefore { get; set; }

        public DateTime? CompletedAfter { get; set; }
        public DateTime? CompletedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }
}
