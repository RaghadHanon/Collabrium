using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.EventsHandlers;

public sealed class UserRoleChangedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<UserRoleChangedEvent>
{
    public async Task Handle(UserRoleChangedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.User.Id))
            return;

        var oldJobTitle = FormatJobTitle(notification.OldJobTitle);
        var newJobTitle = FormatJobTitle(notification.User.JobTitle);

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.User.Id],
            type: NotificationType.UserRoleChanged,
            title: "Role Updated",
            message: $"{notification.AssignerFullName} changed your role from {oldJobTitle} to {newJobTitle}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "User",
            entityId: notification.User.Id,
            actionUrl: "/dashboard/profile",
            cancellationToken: ct);
    }

    private static string FormatJobTitle(string? jobTitle)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
            return string.Empty;

        return jobTitle.Length > 3
            ? Regex.Replace(jobTitle, "(\\B[A-Z])", " $1")
            : jobTitle;
    }
}