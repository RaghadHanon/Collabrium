namespace API.Contracts.Identity;
/// <summary>
/// Request model for updating user profile information.
/// </summary>
public class UpdateUserProfileRequest
{
    // Contact Info
    public string? PhoneNumber { get; set; }
    public string? LinkedIn { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? YouTube { get; set; }
    public string? Github { get; set; }
}
