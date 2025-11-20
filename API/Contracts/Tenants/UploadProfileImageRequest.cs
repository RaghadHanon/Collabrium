namespace API.Contracts.Tenants;

public class UploadProfileImageRequest
{
    public IFormFile ProfileImage { get; set; } = default!;
}
