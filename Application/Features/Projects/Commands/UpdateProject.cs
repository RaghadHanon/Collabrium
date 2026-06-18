using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Commands;

public class UpdateProject
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public required string ProjectId { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public ProjectStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler(
        IProjectsRepository projectsRepository,
        IProjectsReadModel projectsReadModel,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectDto>.Failure("Project not found in this tenant.", HttpStatusCode.NotFound);

            var nameDecision = PatchHelpers.DecideStringPatch(request.Name);

            if (nameDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var newName = nameDecision.Value!;
                if (!string.Equals(project.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    var isUnique = await projectsRepository.IsNameUniqueAsync(tenantId, newName, cancellationToken);
                    if (!isUnique)
                        return Result<ProjectDto>.Failure("A project with this name already exists.", HttpStatusCode.BadRequest);
                }
            }

            var newStartFinal = request.StartDate ?? project.StartDate;
            var newEndFinal = request.EndDate ?? project.EndDate;

            if (newStartFinal.HasValue && newEndFinal.HasValue && newEndFinal.Value < newStartFinal.Value)
                return Result<ProjectDto>.Failure("EndDate cannot be before StartDate.", HttpStatusCode.BadRequest);

            var oldProjectState = project.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                project.UpdateDetails(
                    name: PatchHelpers.ApplyStringPatch(project.Name, request.Name) ?? project.Name,
                    description: PatchHelpers.ApplyStringPatch(project.Description, request.Description),
                    startDate: newStartFinal,
                    endDate: newEndFinal
                );

                var projectsWorkItems = await projectsReadModel.GetProjectWorkItemSnapshotAsync(tenantId, project.Id, cancellationToken);

                if (request.Status.HasValue)
                    project.ChangeStatusWithValidation(request.Status.Value, projectsWorkItems);

                await projectsRepository.UpdateAsync(project, cancellationToken);

                historyRecorder.RecordUpdatingChangesAsync<Project>(project.Id, project.Id, project.Name, project.Key, oldProjectState, project);
            }, cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, project.Id, cancellationToken);

            return dto is null
                ? Result<ProjectDto>.Failure("Project updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}