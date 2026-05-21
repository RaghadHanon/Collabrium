using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Events.Queries;
public sealed class GetEventDetails
{
    public sealed class Query : IRequest<Result<EventDetailsDto>>
    {
        public required string EventId { get; set; }
    }

    public sealed class Handler(
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<EventDetailsDto>>
    {
        public async Task<Result<EventDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<EventDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            var dto = await eventsReadModel.GetDetailsAsync(
                tenantId,
                request.EventId,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDetailsDto>.Failure("Event not found.", HttpStatusCode.NotFound)
                : Result<EventDetailsDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}