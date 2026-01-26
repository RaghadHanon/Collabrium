using Infrastructure.Persistence.Entities.Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class TeamMemberMapper
{
    public static TeamMemberEntity ToEntity(string teamId, string userId, DateTime? joinedAt = null)
    {
        return string.IsNullOrWhiteSpace(teamId)
            ? throw new ArgumentNullException(nameof(teamId))
            : string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentNullException(nameof(userId))
            : new TeamMemberEntity
            {
                TeamId = teamId,
                UserId = userId,
                JoinedAt = joinedAt ?? DateTime.UtcNow
            };
    }
}