namespace API.Contracts.UserCourses
{
    public sealed class GetMySavedUserCoursesRequest
    {
        public string? Search { get; set; }

        public DateTime? AssignedAfter { get; set; }
        public DateTime? AssignedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }
}
