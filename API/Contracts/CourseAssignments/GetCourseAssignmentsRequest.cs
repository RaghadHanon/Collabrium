namespace API.Contracts.CourseAssignments
{
    public class GetCourseAssignmentsRequest
    {
        public string? CourseId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedByUserId { get; set; }

        public DateTime? AssignedAfter { get; set; }
        public DateTime? AssignedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }


    }
}
