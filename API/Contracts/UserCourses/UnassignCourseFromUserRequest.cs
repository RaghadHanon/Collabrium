namespace API.Contracts.UserCourses;

public sealed class UnassignCourseFromUserRequest
{
    public required string AssignedToUserId { get; set; }
}
