namespace API.Contracts.Identity;

/// <summary>
/// Send Password Reset Email Request
/// </summary>
public class SendPasswordResetEmailRequest
{
    /// <summary>
    /// The user email
    /// </summary>
    public required string Email { get; set; }
}
