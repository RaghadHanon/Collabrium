namespace Application.Features.Courses.DTOs
{
    public sealed class CourseDetailsDto : CourseDto
    {
        public List<string> LearningPoints { get; set; } = new();

        public int AssignmentsCount { get; set; }
        public int UserProgressCount { get; set; }
    }

}
