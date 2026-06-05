using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Notifications;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Notifications.Commands;

public sealed class MarkAllNotificationsAsRead
{
    public sealed class Command : IRequest<Result<string>>
    {
    }

    public sealed class Handler(
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        INotificationsRepository notificationsRepository,
        INotificationsReadModel notificationsReadModel,
        INotificationRealtimeService notificationRealtimeService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser is null)
                return Result<string>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await notificationsRepository.MarkAllAsReadAsync(
                    tenantId,
                    currentUser.Id,
                    ct);
            }, cancellationToken);

            var unreadCount = await notificationsReadModel.GetUnreadCountAsync(
                tenantId,
                currentUser.Id,
                cancellationToken);

            await notificationRealtimeService.SendUnreadCountChangedAsync(
                tenantId,
                currentUser.Id,
                unreadCount,
                cancellationToken);

            return Result<string>.Success(
                "All notifications marked as read.",
                HttpStatusCode.OK);
        }
    }
}