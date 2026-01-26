namespace Infrastructure.Persistence.Entities;

public class ContactDetailsOwned
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public SocialLinksOwned Socials { get; set; } = new();
}
