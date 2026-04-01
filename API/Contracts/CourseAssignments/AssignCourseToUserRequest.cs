namespace API.Contracts.CourseAssignments
{
    public sealed class AssignCourseToUserRequest
    {
        public required string AssignedToUserId { get; set; }
    }
}
