using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Domain.Models;

namespace Application.Services;

public sealed class ProjectHistoryValueResolver(
    IUsersReadModel usersReadModel,
    ITeamsReadModel teamsReadModel,
    IWorkItemsReadModel workItemsReadModel,
    ISprintsReadModel sprintsReadModel
) : IProjectHistoryValueResolver
{
    public async Task<string?> ResolveAsync(
        string tenantId,
        string projectId,
        string propertyName,
        string? value,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmedValue = value.Trim();

        return propertyName switch
        {
            nameof(Project.CreatedByUserId)
            or nameof(WorkItem.AssignedToUserId)
            or nameof(Attachment.UploadedByUserId)
                => await ResolveUserAsync(
                    tenantId,
                    trimmedValue,
                    cancellationToken),

            nameof(Project.TeamId)
                => await ResolveTeamAsync(
                    tenantId,
                    trimmedValue,
                    cancellationToken),

            nameof(WorkItem.SprintId)
                => await ResolveSprintAsync(
                    tenantId,
                    projectId,
                    trimmedValue,
                    cancellationToken),

            nameof(WorkItem.ParentId)
            or nameof(WorkItemRelation.SourceWorkItemId)
            or nameof(WorkItemRelation.TargetWorkItemId)
                => await ResolveWorkItemAsync(
                    tenantId,
                    projectId,
                    trimmedValue,
                    cancellationToken),

            _ => trimmedValue
        };
    }

    private async Task<string> ResolveUserAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        var displayName = await usersReadModel.GetUserDisplayNameAsync(
            tenantId,
            userId,
            cancellationToken);

        return displayName ?? userId;
    }

    private async Task<string> ResolveTeamAsync(
        string tenantId,
        string teamId,
        CancellationToken cancellationToken)
    {
        var displayName = await teamsReadModel.GetTeamDisplayNameAsync(
            tenantId,
            teamId,
            cancellationToken);

        return displayName ?? teamId;
    }

    private async Task<string> ResolveWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CancellationToken cancellationToken)
    {
        var displayName = await workItemsReadModel.GetWorkItemDisplayNameAsync(
            tenantId,
            projectId,
            workItemId,
            cancellationToken);

        return displayName ?? workItemId;
    }

    private async Task<string> ResolveSprintAsync(
        string tenantId,
        string projectId,
        string sprintId,
        CancellationToken cancellationToken)
    {
        var displayName = await sprintsReadModel.GetSprintDisplayNameAsync(
            tenantId,
            projectId,
            sprintId,
            cancellationToken);

        return displayName ?? sprintId;
    }
}