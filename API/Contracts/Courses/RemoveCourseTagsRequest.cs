namespace API.Contracts.Courses;

public sealed class RemoveCourseTagsRequest
{
    public List<string> TagIds { get; set; } = new();
}
