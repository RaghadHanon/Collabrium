using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Events.Queries;
public sealed class GetEventById
{
    public sealed class Query : IRequest<Result<EventDto>>
    {
        public required string EventId { get; set; }
    }

    public sealed class Handler(
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<EventDto>>
    {
        public async Task<Result<EventDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<EventDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                request.EventId,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Event not found.", HttpStatusCode.NotFound)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}