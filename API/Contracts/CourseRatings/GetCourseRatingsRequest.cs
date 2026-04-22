namespace API.Contracts.CourseRatings
{
    public sealed class GetCourseRatingsRequest
    {
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }
}
