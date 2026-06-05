using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record RemoveMemberFromTeamEvent(
    string TenantId,
    string TeamId,
    List<UserSnapshotDto> TeamMembers,
    string AssignerUserId,
    string AssignerFullName,
    string TeamLeadName,
    string TeamName
) : INotification;