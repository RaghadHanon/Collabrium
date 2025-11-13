namespace API.Contracts.Identity;

/// <summary>
/// ReSend Confirmation Email Request
/// </summary>
public class ReSendConfirmationEmailRequest
{
    /// <summary>
    /// The user Email
    /// </summary>
    public required string Email { get; set; }
}
