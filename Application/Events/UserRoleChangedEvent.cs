using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record UserRoleChangedEvent(
    string TenantId,
    UserSnapshotDto User,
    string OldJobTitle,
    string AssignerUserId,
    string AssignerFullName
) : INotification;