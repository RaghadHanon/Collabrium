using Application.Common;
using Application.Features.Notifications.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Notifications.Queries;

public sealed class GetMyNotifications
{
    public sealed class Query : IRequest<Result<PagedList<NotificationDto, DateTime?>>>
    {
        public DateTime? Cursor { get; init; }
        public int PageSize { get; init; } = 10;
        public bool? IsRead { get; init; }
    }

    public sealed class Handler(
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        INotificationsReadModel notificationsReadModel,
        IMapper mapper
    ) : IRequestHandler<Query, Result<PagedList<NotificationDto, DateTime?>>>
    {
        public async Task<Result<PagedList<NotificationDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<NotificationDto, DateTime?>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser is null)
                return Result<PagedList<NotificationDto, DateTime?>>.Failure(
                    "Current user is not found.",
                    HttpStatusCode.NotFound);

            var parameters = mapper.Map<NotificationsSearchAndFilterParametersDto>(request);

            var notifications = await notificationsReadModel.GetUserNotificationsAsync(
                tenantId,
                currentUser.Id,
                parameters,
                cancellationToken);

            return Result<PagedList<NotificationDto, DateTime?>>.Success(
                notifications,
                HttpStatusCode.OK);
        }
    }
}