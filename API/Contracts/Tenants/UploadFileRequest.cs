namespace API.Contracts.Tenants;

public class UploadFileRequest
{
    public IFormFile File { get; set; } = default!;
}
