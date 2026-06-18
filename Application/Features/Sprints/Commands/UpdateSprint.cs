using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class UpdateSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string SprintId { get; set; }
        public required string ProjectId { get; set; }

        public string? Name { get; set; }
        public string? Goal { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
        ISprintsReadModel sprintsReadModel,
        ITenantContext tenantContext,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId =
                tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<SprintDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var project =
                await projectsRepository.GetByIdAsync(
                    tenantId,
                    request.ProjectId,
                    cancellationToken);

            if (project is null)
            {
                return Result<SprintDto>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);
            }

            var sprint =
                await sprintsRepository.GetByIdAsync(
                    tenantId,
                    request.SprintId,
                    request.ProjectId,
                    cancellationToken);

            if (sprint is null)
            {
                return Result<SprintDto>.Failure(
                    "Sprint not found.",
                    HttpStatusCode.NotFound);
            }

            if (sprint.Status is
                Domain.Enums.SprintStatus.Completed or
                Domain.Enums.SprintStatus.Cancelled)
            {
                return Result<SprintDto>.Failure(
                    $"Cannot update a {sprint.Status} sprint.",
                    HttpStatusCode.BadRequest);
            }

            var scheduleChangeRequested =
                request.StartDate.HasValue ||
                request.EndDate.HasValue;

            if (scheduleChangeRequested)
            {
                var scheduleIsNotInitialized =
                    sprint.StartDate == default ||
                    sprint.EndDate == default;

                if (scheduleIsNotInitialized &&
                    (!request.StartDate.HasValue ||
                     !request.EndDate.HasValue))
                {
                    return Result<SprintDto>.Failure(
                        "Both start date and end date are required when scheduling the sprint for the first time.",
                        HttpStatusCode.BadRequest);
                }

                var effectiveStartDate =
                    request.StartDate ??
                    sprint.StartDate;

                var effectiveEndDate =
                    request.EndDate ??
                    sprint.EndDate;

                if (effectiveEndDate <= effectiveStartDate)
                {
                    return Result<SprintDto>.Failure(
                        "Sprint end date must be greater than the start date.",
                        HttpStatusCode.BadRequest);
                }

                if (project.StartDate.HasValue &&
                    effectiveStartDate < project.StartDate.Value)
                {
                    return Result<SprintDto>.Failure(
                        "Sprint start date cannot be before the project start date.",
                        HttpStatusCode.BadRequest);
                }

                if (project.EndDate.HasValue &&
                    effectiveEndDate > project.EndDate.Value)
                {
                    return Result<SprintDto>.Failure(
                        "Sprint end date cannot be after the project end date.",
                        HttpStatusCode.BadRequest);
                }
            }

            var updatedName =
                PatchHelpers.ApplyStringPatch(
                    sprint.Name,
                    request.Name);

            var updatedGoal =
                PatchHelpers.ApplyStringPatch(
                    sprint.Goal,
                    request.Goal);

            var oldSprintState =
                sprint.Clone();

            await unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    sprint.Update(
                        start: request.StartDate,
                        end: request.EndDate,
                        name: updatedName,
                        goal: updatedGoal);

                    await sprintsRepository.UpdateAsync(
                        sprint,
                        transactionCancellationToken);

                    await historyRecorder.RecordUpdatingChangesAsync(
                        projectId: sprint.ProjectId,
                        relatedEntityId: sprint.Id,
                        relatedEntityTitle: sprint.Name ?? sprint.Code,
                        relatedEntityKey: sprint.Code,
                        oldObject: oldSprintState,
                        newObject: sprint,
                        cancellationToken: transactionCancellationToken);
                },
                cancellationToken);

            var dto =
                await sprintsReadModel.GetByIdAsync(
                    tenantId,
                    sprint.Id,
                    request.ProjectId,
                    cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure(
                    "Sprint updated but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(
                    dto,
                    HttpStatusCode.OK);
        }
    }
}