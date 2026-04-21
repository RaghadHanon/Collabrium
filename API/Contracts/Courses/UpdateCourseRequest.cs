using Domain.Enums;

namespace API.Contracts.Courses
{
    public sealed class UpdateCourseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Provider { get; set; }
        public string? InstructorName { get; set; }

        public CourseLevel? Level { get; set; }
        public CourseLanguage? Language { get; set; }

        public int? EstimatedDurationInHours { get; set; }
        public List<string>? LearningPoints { get; set; }
    }
}
