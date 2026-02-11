using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Boards;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProjectBoard
{
    public class Query : IRequest<Result<BoardDto>>
    {
        public required string ProjectId { get; set; }
        public string? SprintId { get; set; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<BoardDto>>
    {
        public async Task<Result<BoardDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<BoardDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var items = await workItemsReadModel.GetBoardItemsAsync(
                tenantId: tenantId,
                projectId: request.ProjectId,
                sprintId: request.SprintId,
                cancellationToken: cancellationToken);

            var dto = new BoardDto
            {
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                Columns = BuildColumns(items)
            };

            return Result<BoardDto>.Success(dto, HttpStatusCode.OK);
        }

        private static List<BoardColumnDto> BuildColumns(List<WorkItemListItemDto> items)
        {
            var statuses = new[]
            {
                WorkItemStatus.ToDo,
                WorkItemStatus.ReadyForDev,
                WorkItemStatus.InProgress,
                WorkItemStatus.InReview,
                WorkItemStatus.InTesting,
                WorkItemStatus.ReadyForRelease,
                WorkItemStatus.Done
            };

            return statuses.Select(s => new BoardColumnDto
            {
                Status = s,
                Title = s.ToString(),
                Items = items.Where(x => x.Status == s).ToList()
            }).ToList();
        }
    }
}