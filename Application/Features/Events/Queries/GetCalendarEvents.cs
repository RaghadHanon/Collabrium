using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Events.Queries;
public sealed class GetCalendarEvents
{
    public sealed class Query : IRequest<Result<List<EventCalendarItemDto>>>
    {
        public DateTime StartsAfter { get; set; }
        public DateTime StartsBefore { get; set; }
    }

    public sealed class Handler(
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<List<EventCalendarItemDto>>>
    {
        public async Task<Result<List<EventCalendarItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<EventCalendarItemDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (request.StartsBefore <= request.StartsAfter)
                return Result<List<EventCalendarItemDto>>.Failure("StartsBefore must be after StartsAfter.", HttpStatusCode.BadRequest);

            var result = await eventsReadModel.GetCalendarEventsAsync(
                tenantId,
                request.StartsAfter,
                request.StartsBefore,
                cancellationToken);

            return Result<List<EventCalendarItemDto>>.Success(result, HttpStatusCode.OK);
        }
    }
}