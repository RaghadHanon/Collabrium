namespace API.Contracts.Courses;

public sealed class SetCourseTagsRequest
{
    public List<string> TagIds { get; set; } = new();
}
