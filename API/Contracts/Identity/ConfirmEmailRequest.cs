namespace API.Contracts.Identity;

/// <summary>
/// A request to confirm email
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// The user id from the url.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// The token from the url.
    /// </summary>
    public required string Token { get; set; }
}
