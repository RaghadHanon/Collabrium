using Application.Features.ProjectsHistories.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using System.Text.RegularExpressions;

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
            nameof(Project.LastSprintNumber),

            nameof(WorkItem.ProjectId),

            nameof(WorkItem.Relations),
            nameof(WorkItem.Comments),
            nameof(WorkItem.CodeLinks),
            nameof(WorkItem.Attachments),
            nameof(WorkItem.IsDone)
        };

    public async Task<IReadOnlyList<RecordedProjectHistoryChangeDto>>
        RecordUpdatingChangesAsync<T>(
            string projectId,
            string relatedEntityId,
            string relatedEntityTitle,
            string relatedEntityKey,
            T oldObject,
            T newObject,
            CancellationToken cancellationToken = default)
    {
        var recordedChanges =
            new List<RecordedProjectHistoryChangeDto>();

        var currentUserId =
            currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return recordedChanges;

        var tenantId =
            tenantContext.CurrentTenant?.Id
            ?? tenantContext.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            return recordedChanges;

        if (oldObject is null || newObject is null)
            return recordedChanges;

        var oldObjectType = oldObject.GetType();
        var newObjectType = newObject.GetType();

        if (oldObjectType != newObjectType)
        {
            throw new ArgumentException(
                "Old and new objects must have the same runtime type.");
        }

        var scope =
            DetermineScope(newObjectType);

        var properties =
            newObjectType.GetProperties();

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            if (IgnoredProperties.Contains(property.Name))
                continue;

            if (property.PropertyType != typeof(string) &&
                typeof(System.Collections.IEnumerable)
                    .IsAssignableFrom(property.PropertyType))
            {
                continue;
            }

            var oldRawValue =
                property.GetValue(oldObject)?.ToString();

            var newRawValue =
                property.GetValue(newObject)?.ToString();

            if (string.Equals(
                    oldRawValue,
                    newRawValue,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var oldDisplayValue =
                await valueResolver.ResolveAsync(
                    tenantId: tenantId,
                    projectId: projectId,
                    propertyName: property.Name,
                    value: oldRawValue,
                    cancellationToken: cancellationToken);

            var newDisplayValue =
                await valueResolver.ResolveAsync(
                    tenantId: tenantId,
                    projectId: projectId,
                    propertyName: property.Name,
                    value: newRawValue,
                    cancellationToken: cancellationToken);

            var displayPropertyName =
                FormatPropertyName(property.Name);

            var history = new ProjectHistory(
                projectId: projectId,
                relatedEntityId: relatedEntityId,
                relatedEntityTitle: relatedEntityTitle,
                relatedEntityCode: relatedEntityKey,
                scope: scope,
                actionType: ActionType.Update,
                propertyName: displayPropertyName,
                oldValue: oldDisplayValue,
                newValue: newDisplayValue,
                madeByUserId: currentUserId
            );

            projectHistoryRepository.Add(history);

            recordedChanges.Add(
                new RecordedProjectHistoryChangeDto(
                    PropertyName: displayPropertyName,
                    OldValue: oldDisplayValue,
                    NewValue: newDisplayValue));
        }

        return recordedChanges;
    }

    public void RecordCreationChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey)
    {
        var currentUserId =
            currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return;

        var scope =
            DetermineScope(typeof(T));

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
        var currentUserId =
            currentUserAccessor.GetUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
            return;

        var scope =
            DetermineScope(typeof(T));

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

    private static string FormatPropertyName(
        string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return propertyName;

        var nameWithoutId =
            propertyName.EndsWith(
                "Id",
                StringComparison.OrdinalIgnoreCase)
                ? propertyName[..^2]
                : propertyName;

        return Regex.Replace(
                nameWithoutId,
                "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])",
                " ")
            .Trim();
    }

    private static ProjectHistoryScope DetermineScope(
        Type entityType)
    {
        if (typeof(Project).IsAssignableFrom(entityType))
            return ProjectHistoryScope.Project;

        if (typeof(Sprint).IsAssignableFrom(entityType))
            return ProjectHistoryScope.Sprint;

        if (typeof(WorkItem).IsAssignableFrom(entityType))
            return ProjectHistoryScope.WorkItem;

        if (typeof(WorkItemRelation).IsAssignableFrom(entityType))
            return ProjectHistoryScope.WorkItemRelation;

        if (typeof(CodeLink).IsAssignableFrom(entityType))
            return ProjectHistoryScope.WorkItemCodeLink;

        if (typeof(WorkItemComment).IsAssignableFrom(entityType))
            return ProjectHistoryScope.WorkItemComment;

        return typeof(Attachment).IsAssignableFrom(entityType)
            ? ProjectHistoryScope.WorkItemAttachment
            : throw new ArgumentException(
                $"Unsupported project history type: {entityType.Name}");
    }
}