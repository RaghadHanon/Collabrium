using Application.Common;
using Application.Features.Projects.DTOs.Boards;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
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
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<BoardDto>>
    {
        public async Task<Result<BoardDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<BoardDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(request.SprintId) && !await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, request.ProjectId))
                return Result<BoardDto>.Failure("Sprint not found in this project.", HttpStatusCode.NotFound);

            var items = await workItemsReadModel.GetBoardItemsAsync(
                tenantId: tenantId,
                projectId: request.ProjectId,
                sprintId: request.SprintId,
                cancellationToken: cancellationToken);

            var userStories = await workItemsReadModel.GetBoardUserStoriesAsync(
                tenantId: tenantId,
                projectId: request.ProjectId,
                sprintId: request.SprintId,
                cancellationToken: cancellationToken);

            var dto = new BoardDto
            {
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                UserStoryRows = BuildUserStoryGroups(items, userStories),
                OrphanRow = BuildOrphanGroups(items),
            };

            return Result<BoardDto>.Success(dto, HttpStatusCode.OK);
        }

        private static List<UserStoryBoardGroupDto> BuildUserStoryGroups(
            List<WorkItemListItemDto> items,
            List<WorkItemListItemDto> userStories)
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

            return userStories.Select(us => new UserStoryBoardGroupDto
            {
                ParentUserStory = us,

                Columns = statuses.Select(s => new BoardColumnDto
                {
                    Status = s,
                    Title = s.ToString(),
                    Items = items
                        .Where(i => i.Parent?.TargetWorkItem?.Id == us.Id && i.Status == s)
                        .ToList()
                }).ToList()
            }).ToList();
        }

        private static OrphanBoardGroupDto BuildOrphanGroups(
            List<WorkItemListItemDto> items)
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

            return new OrphanBoardGroupDto
            {
                Columns = statuses.Select(s => new BoardColumnDto
                {
                    Status = s,
                    Title = s.ToString(),
                    Items = items
                        .Where(i => string.IsNullOrEmpty(i.Parent?.TargetWorkItem?.Id) && i.Status == s)
                        .ToList()
                }).ToList()
            };
        }
    }
}