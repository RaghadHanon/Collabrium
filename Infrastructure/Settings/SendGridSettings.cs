namespace Infrastructure.Settings;

public class SendGridSettings
{
    public required string ApiKey { get; set; }
    public required string From { get; set; }
}