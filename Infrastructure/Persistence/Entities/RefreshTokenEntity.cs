using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities;

public class RefreshTokenEntity : DbEntity
{
    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUserEntity User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    [NotMapped]
    public bool IsRevoked => RevokedAtUtc.HasValue;

    [NotMapped]
    public bool IsActive => !IsExpired && !IsRevoked;
}