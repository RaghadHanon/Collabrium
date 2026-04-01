namespace API.Contracts.Identity;

public class RevokeRefreshTokenRequest
{
    public required string RefreshToken { get; set; }
}