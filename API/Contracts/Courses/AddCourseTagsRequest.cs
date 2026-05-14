namespace API.Contracts.Courses;

public sealed class AddCourseTagsRequest
{
    public List<string> TagIds { get; set; } = new();
}
