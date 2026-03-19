namespace Infrastructure.Persistence.Entities
{
    public sealed class CourseTagEntity
    {
        public string CourseId { get; set; } = null!;
        public string TagId { get; set; } = null!;

        public CourseEntity Course { get; set; } = null!;
        public TagEntity Tag { get; set; } = null!;
    }
}
