namespace API.Contracts.Tenants;

public class UploadImageRequest
{
    public IFormFile File { get; set; } = default!;
}
