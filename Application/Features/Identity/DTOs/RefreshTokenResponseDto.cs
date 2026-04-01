namespace Application.Features.Identity.DTOs;

public sealed record RefreshTokenResponseDto
{
    /// <summary>
    /// New JWT Access token issued upon successful refresh
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// New JWT Refresh token issued alongside the new access token, used to obtain new access tokens without re-authenticating
    /// </summary>
    public required string RefreshToken { get; init; }
}