using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Events.Queries;
public sealed class GetManageEvents
{
    public sealed class Query : IRequest<Result<PagedList<EventDto, DateTime?>>>
    {
        public string? Search { get; set; }

        public bool? IsOnline { get; set; }
        public bool? IsCancelled { get; set; }
        public bool? HasAvailableSeats { get; set; }

        public DateTime? StartsAfter { get; set; }
        public DateTime? StartsBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }

    public sealed class Handler(
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext,
        IMapper mapper,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<PagedList<EventDto, DateTime?>>>
    {
        public async Task<Result<PagedList<EventDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<EventDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            var parameters = mapper.Map<EventsSearchAndFilterParametersDto>(request);
            parameters.TenantId = tenantId;
            parameters.CurrentUserId = currentUserId;
            var result = await eventsReadModel.GetManageEventsAsync(parameters, cancellationToken);

            return Result<PagedList<EventDto, DateTime?>>.Success(result, HttpStatusCode.OK);
        }
    }
}