using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;

namespace Application.Services;

public sealed class ProjectHistoryRecorder(
    IProjectHistoryRepository projectHistoryRepository,
    ICurrentUserAccessor currentUserAccessor,
    ITenantContext tenantContext,
    IProjectHistoryValueResolver valueResolver
) : IProjectHistoryRecorder
{
    private static readonly HashSet<string> IgnoredProperties =
        new(StringComparer.Ordinal)
        {
            nameof(Entity.Id),
            nameof(Entity.CreatedAt),
            nameof(Entity.UpdatedAt),
            nameof(Project.TenantId),
            nameof(Project.LastWorkItemNumber),
            nameof(Project.LastSprintNumber)
        };

    public async Task RecordUpdatingChangesAsync<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey,
        T oldObject,
        T newObject,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return;

        var tenantId = tenantContext.CurrentTenant?.Id
                       ?? tenantContext.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            return;

        if (oldObject is null || newObject is null)
            return;

        var scope = DetermineScope<T>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            if (IgnoredProperties.Contains(property.Name))
                continue;

            var oldRawValue = property.GetValue(oldObject)?.ToString();
            var newRawValue = property.GetValue(newObject)?.ToString();

            if (string.Equals(
                    oldRawValue,
                    newRawValue,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var oldDisplayValue = await valueResolver.ResolveAsync(
                tenantId: tenantId,
                projectId: projectId,
                propertyName: property.Name,
                value: oldRawValue,
                cancellationToken: cancellationToken);

            var newDisplayValue = await valueResolver.ResolveAsync(
                tenantId: tenantId,
                projectId: projectId,
                propertyName: property.Name,
                value: newRawValue,
                cancellationToken: cancellationToken);

            var history = new ProjectHistory(
                projectId: projectId,
                relatedEntityId: relatedEntityId,
                relatedEntityTitle: relatedEntityTitle,
                relatedEntityCode: relatedEntityKey,
                scope: scope,
                actionType: ActionType.Update,
                propertyName: property.Name,
                oldValue: oldDisplayValue,
                newValue: newDisplayValue,
                madeByUserId: currentUserId
            );

            projectHistoryRepository.Add(history);
        }
    }

    public void RecordCreationChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey)
    {
        var currentUserId = currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return;

        var scope = DetermineScope<T>();

        var history = new ProjectHistory(
            projectId: projectId,
            relatedEntityId: relatedEntityId,
            relatedEntityTitle: relatedEntityTitle,
            relatedEntityCode: relatedEntityKey,
            scope: scope,
            actionType: ActionType.Create,
            propertyName: null,
            oldValue: null,
            newValue: null,
            madeByUserId: currentUserId
        );

        projectHistoryRepository.Add(history);
    }

    public void RecordDeletionChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey)
    {
        var currentUserId = currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return;

        var scope = DetermineScope<T>();

        var history = new ProjectHistory(
            projectId: projectId,
            relatedEntityId: relatedEntityId,
            relatedEntityTitle: relatedEntityTitle,
            relatedEntityCode: relatedEntityKey,
            scope: scope,
            actionType: ActionType.Delete,
            propertyName: null,
            oldValue: null,
            newValue: null,
            madeByUserId: currentUserId
        );

        projectHistoryRepository.Add(history);
    }

    private static ProjectHistoryScope DetermineScope<T>()
    {
        return typeof(T).Name switch
        {
            nameof(Project) =>
                ProjectHistoryScope.Project,

            nameof(Sprint) =>
                ProjectHistoryScope.Sprint,

            nameof(WorkItem) =>
                ProjectHistoryScope.WorkItem,

            nameof(WorkItemRelation) =>
                ProjectHistoryScope.WorkItemRelation,

            nameof(CodeLink) =>
                ProjectHistoryScope.WorkItemCodeLink,

            nameof(WorkItemComment) =>
                ProjectHistoryScope.WorkItemComment,

            nameof(Attachment) =>
                ProjectHistoryScope.WorkItemAttachment,

            _ => throw new ArgumentException(
                $"Unsupported project history type: {typeof(T).Name}")
        };
    }
}