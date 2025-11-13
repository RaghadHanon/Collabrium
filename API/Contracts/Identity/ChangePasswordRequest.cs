namespace API.Contracts.Identity;

/// <summary>
/// A request to change the password.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// The current password of the user
    /// </summary>
    public required string CurrentPassword { get; set; }
    /// <summary>
    /// The new password to set
    /// </summary>
    public required string NewPassword { get; set; }
}
