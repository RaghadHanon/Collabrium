namespace Application.Features.Courses.DTOs
{
    public sealed class CourseDetailsDto : CourseDto
    {
        public List<string> LearningPoints { get; set; } = new();

    }

}
