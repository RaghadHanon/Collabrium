using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record CourseAssignedToTeamEvent(
    string TenantId,
    string TeamId,
    string TeamName,
    string CourseId,
    string CourseTitle,
    string CourseProvider,
    string AssignerUserId,
    string AssignerFullName,
    List<UserSnapshotDto> Members
) : INotification;