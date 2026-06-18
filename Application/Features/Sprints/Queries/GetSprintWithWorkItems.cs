using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Queries;

public sealed class GetSprintWithWorkItems
{
    public sealed class Query :
        IRequest<Result<SprintWithWorkItemsDto>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public sealed class Handler(
        ISprintsReadModel sprintsReadModel,
        IReportsReadModel reportsReadModel,
        ITenantContext tenantContext)
        : IRequestHandler<
            Query,
            Result<SprintWithWorkItemsDto>>
    {
        public async Task<Result<SprintWithWorkItemsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<SprintWithWorkItemsDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(request.ProjectId))
            {
                return Result<SprintWithWorkItemsDto>.Failure(
                    "Project id is required.",
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(request.SprintId))
            {
                return Result<SprintWithWorkItemsDto>.Failure(
                    "Sprint id is required.",
                    HttpStatusCode.BadRequest);
            }

            var sprint = await sprintsReadModel
                .GetSprintWithWorkItemsAsync(
                    tenantId,
                    request.ProjectId,
                    request.SprintId,
                    cancellationToken);

            if (sprint is null)
            {
                return Result<SprintWithWorkItemsDto>.Failure(
                    "Sprint not found.",
                    HttpStatusCode.NotFound);
            }

            var sprintProgress = await reportsReadModel
                .GenerateSprintProgressReport(
                    tenantId,
                    request.ProjectId,
                    request.SprintId);

            sprint.SprintProgress = sprintProgress;

            return Result<SprintWithWorkItemsDto>.Success(
                sprint,
                HttpStatusCode.OK);
        }
    }
}