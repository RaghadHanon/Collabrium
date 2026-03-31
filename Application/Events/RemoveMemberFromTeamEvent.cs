using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record RemoveMemberFromTeamEvent(
    string TeamId,
    List<UserSnapshotDto> TeamMembers,
    string AssignerFullName,
    string TeamLeadName,
    string TeamName
) : INotification;