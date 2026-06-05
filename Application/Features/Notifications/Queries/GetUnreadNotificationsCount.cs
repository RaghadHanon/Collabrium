using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Notifications.Queries;

public sealed class GetUnreadNotificationsCount
{
    public sealed class Query : IRequest<Result<int>>;

    public sealed class Handler(
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        INotificationsReadModel notificationsReadModel
    ) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<int>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser is null)
                return Result<int>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var count = await notificationsReadModel.GetUnreadCountAsync(
                tenantId,
                currentUser.Id,
                cancellationToken);

            return Result<int>.Success(count, HttpStatusCode.OK);
        }
    }
}