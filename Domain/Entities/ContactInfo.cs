namespace Domain.Models;

public abstract class ContactInfo : Entity
{
    public string? LinkedIn { get; private set; } = string.Empty;
    public string? Facebook { get; private set; } = string.Empty;
    public string? Instagram { get; private set; } = string.Empty;
    public string? YouTube { get; private set; } = string.Empty;

    public ContactInfo() { }

    public void UpdateSocials(string? linkedIn, string? facebook, string? instagram, string? youTube)
    {
        LinkedIn = linkedIn ?? string.Empty;
        Facebook = facebook ?? string.Empty;
        Instagram = instagram ?? string.Empty;
        YouTube = youTube ?? string.Empty;
    }
}
