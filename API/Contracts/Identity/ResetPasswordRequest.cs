namespace API.Contracts.Identity;

/// <summary>
/// A request to reset the password
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// The user id from the url.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// The token from the url.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The new password
    /// </summary>
    public required string NewPassword { get; set; }
}

