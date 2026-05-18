using Domain.Core;
using Domain.Enums;

namespace Domain.Models;
public sealed class AnnouncementAudience : Entity
{
    public string AnnouncementId { get; private set; } = null!;

    public AnnouncementAudienceType Type { get; private set; }

    public string? TeamId { get; private set; }
    public string? RoleName { get; private set; }
    public string? UserId { get; private set; }

    private AnnouncementAudience() { }

    private AnnouncementAudience(
        string announcementId,
        AnnouncementAudienceType type,
        string? teamId,
        string? roleName,
        string? userId)
    {
        AnnouncementId = Guard.NotNullOrWhiteSpace(announcementId, nameof(announcementId)).Trim();
        Type = type;

        TeamId = string.IsNullOrWhiteSpace(teamId) ? null : teamId.Trim();
        RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim();
        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();

        ValidateTarget();

        Touch();
    }

    public static AnnouncementAudience ForEveryone(string announcementId)
    {
        return new AnnouncementAudience(
            announcementId,
            AnnouncementAudienceType.Everyone,
            null,
            null,
            null);
    }

    public static AnnouncementAudience ForTeam(string announcementId, string teamId)
    {
        return new AnnouncementAudience(
            announcementId,
            AnnouncementAudienceType.Team,
            teamId,
            null,
            null);
    }

    public static AnnouncementAudience ForRole(string announcementId, string roleName)
    {
        return new AnnouncementAudience(
            announcementId,
            AnnouncementAudienceType.Role,
            null,
            roleName,
            null);
    }



    public static AnnouncementAudience Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string announcementId,
        AnnouncementAudienceType type,
        string? teamId,
        string? roleName,
        string? userId)
    {
        var audience = new AnnouncementAudience(
            announcementId,
            type,
            teamId,
            roleName,
            userId);

        audience.Id = id;
        audience.SetTimestamps(createdAt, updatedAt);

        return audience;
    }

    public void AttachToAnnouncement(string announcementId)
    {
        AnnouncementId = Guard.NotNullOrWhiteSpace(announcementId, nameof(announcementId)).Trim();

        ValidateTarget();

        Touch();
    }

    private void ValidateTarget()
    {
        switch (Type)
        {
            case AnnouncementAudienceType.Everyone:
                Guard.Against(
                    TeamId is not null ||
                    RoleName is not null ||
                    UserId is not null,
                    "Everyone audience must not have a specific target.");
                break;

            case AnnouncementAudienceType.Team:
                Guard.Against(
                    string.IsNullOrWhiteSpace(TeamId),
                    "Team audience requires TeamId.");

                Guard.Against(
                    RoleName is not null ||
                    UserId is not null,
                    "Team audience must only have TeamId.");
                break;

            case AnnouncementAudienceType.Role:
                Guard.Against(
                    string.IsNullOrWhiteSpace(RoleName),
                    "Role audience requires RoleName.");

                Guard.Against(
                    TeamId is not null ||
                    UserId is not null,
                    "Role audience must only have RoleName.");
                break;



            default:
                throw new DomainException("Invalid announcement audience type.");
        }
    }
}