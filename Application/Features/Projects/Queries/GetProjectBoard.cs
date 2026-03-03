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

            var allItems = await workItemsReadModel.GetBoardItemsAsync(
                parameters,
                cancellationToken);

            var userStories = allItems
                .Where(x => x.Type == WorkItemType.UserStory)
                .ToList();

            var nonUserStoryItemsWithParents = allItems
                .Where(x => x.Type != WorkItemType.UserStory && x.Parent != null)
                .ToList();

            var orphanNonUserStoryItems = allItems
                .Where(x => x.Type != WorkItemType.UserStory && x.Parent == null)
                .ToList();

            var itemsByParent = nonUserStoryItemsWithParents
                .GroupBy(i => i.Parent!.TargetWorkItem!.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dto = new BoardDto
            {
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                UserStoryRows = BuildUserStoryRows(userStories, itemsByParent),
                OrphanRow = BuildOrphanRow(orphanNonUserStoryItems),
            };

            return Result<BoardDto>.Success(dto, HttpStatusCode.OK);
        }

        private static readonly WorkItemStatus[] BoardStatuses =
        {
            WorkItemStatus.ToDo,
            WorkItemStatus.ReadyForDev,
            WorkItemStatus.InProgress,
            WorkItemStatus.InReview,
            WorkItemStatus.InTesting,
            WorkItemStatus.ReadyForRelease,
            WorkItemStatus.Done
        };

        private List<UserStoryBoardGroupDto> BuildUserStoryRows(
            List<WorkItemListItemDto> userStories,
            Dictionary<string, List<WorkItemListItemDto>> itemsByParent)
        {
            var rows = new List<UserStoryBoardGroupDto>();

            foreach (var us in userStories)
            {
                itemsByParent.TryGetValue(us.Id, out var children);
                children ??= new List<WorkItemListItemDto>();

                var columns = children.Count != 0 ? BoardStatuses
                    .Select(status =>
                    {
                        var items = children
                            .Where(i => i.Status == status)
                            .ToList();

                        return new BoardColumnDto
                        {
                            Status = status,
                            Title = Regex.Replace(status.ToString(), "(\\B[A-Z])", " $1"),
                            Count = items.Count,
                            Items = items
                        };
                    })
                    .ToList() : null;

                rows.Add(new UserStoryBoardGroupDto
                {
                    ParentUserStory = us,
                    Columns = columns
                });
            }

            return rows;
        }

        private static OrphanBoardGroupDto BuildOrphanRow(
            List<WorkItemListItemDto> items)
        {

            return new OrphanBoardGroupDto
            {
                Columns = items.Count != 0 ? BoardStatuses.Select(status => new BoardColumnDto
                {
                    Status = status,
                    Title = Regex.Replace(status.ToString(), "(\\B[A-Z])", " $1"),
                    Items = items
                        .Where(i => string.IsNullOrEmpty(i.Parent?.TargetWorkItem?.Id) && i.Status == status)
                        .ToList()
                }).ToList() : null
            };
        }
    }
}
