namespace API.Contracts.Courses
{
    public sealed class UploadCourseThumbnailRequest
    {
        public required IFormFile File { get; set; }
    }
}
