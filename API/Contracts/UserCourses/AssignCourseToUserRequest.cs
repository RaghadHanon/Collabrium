namespace API.Contracts.UserCourses
{
    public sealed class AssignCourseToUserRequest
    {
        public required string AssignedToUserId { get; set; }
    }
}
