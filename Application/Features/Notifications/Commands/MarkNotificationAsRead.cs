using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

public sealed class MarkNotificationAsRead
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string NotificationId { get; init; }
    }

    public sealed class Handler(
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        INotificationsRepository notificationsRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser is null)
                return Result<string>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await notificationsRepository.MarkAsReadAsync(
                    tenantId,
                    request.NotificationId,
                    currentUser.Id,
                    ct);
            }, cancellationToken);

            return Result<string>.Success("Notification marked as read.", HttpStatusCode.OK);
        }
    }
}