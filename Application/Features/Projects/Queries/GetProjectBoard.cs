using Application.Common;
using Application.Features.Projects.DTOs.Boards;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;
using System.Text.RegularExpressions;

namespace Application.Features.Projects.Queries;

public class GetProjectBoard
{
    public class Query : IRequest<Result<BoardDto>>
    {
        public required string ProjectId { get; set; }
        public string? SprintId { get; set; }

        public WorkItemType? Type { get; set; }
        public WorkItemStatus? Status { get; set; }
        public WorkItemPriority? Priority { get; set; }
        public string? Code { get; set; }

        public bool? IsAssigned { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? TitleContains { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext,
        IMapper mapper)
        : IRequestHandler<Query, Result<BoardDto>>
    {
        public async Task<Result<BoardDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<BoardDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(request.SprintId) &&
                !await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, request.ProjectId))
                return Result<BoardDto>.Failure("Sprint not found in this project.", HttpStatusCode.NotFound);

            if (request.Type.HasValue &&
                request.Type != WorkItemType.Bug &&
                request.Type != WorkItemType.TaskItem &&
                request.Type != WorkItemType.TestCase)
                return Result<BoardDto>.Failure("Invalid work item type for board. Only Bug, Task and Test Case are allowed.", HttpStatusCode.BadRequest);

            if (request.SprintId is not null)
            {
                var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId);
                if (sprint is null)
                    return Result<BoardDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

                if (sprint.Status != SprintStatus.Active)
                    return Result<BoardDto>.Failure("Only active sprints can be viewed on the board.", HttpStatusCode.BadRequest);
            }

            var parameters = mapper.Map<WorkItemsSearchAndFilterParametersDto>(request);
            parameters.TenantId = tenantId;

            var boardDto = await workItemsReadModel.GetBoardItemsAsync(
                parameters,
                cancellationToken);

            return Result<BoardDto>.Success(boardDto, HttpStatusCode.OK);
        }
    }
}
