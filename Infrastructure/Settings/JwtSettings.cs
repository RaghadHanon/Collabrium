namespace Infrastructure.Settings;

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public int ExpiryMinutes { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}