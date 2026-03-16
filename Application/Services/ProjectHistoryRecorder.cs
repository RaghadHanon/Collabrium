using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;

namespace Application.Services;
public class ProjectHistoryRecorder(
    IProjectHistoryRepository projectHistoryRepository,
    ICurrentUserAccessor currentUserAccessor
    ) : IProjectHistoryRecorder
{
    public void RecordUpdatingChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey,
        T oldObject,
        T newObject)
    {
        var currentUserId = currentUserAccessor.GetUserId();
        if (currentUserId == null)
            return;

        var scope = DetermineScope<T>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var oldValue = prop.GetValue(oldObject)?.ToString();
            var newValue = prop.GetValue(newObject)?.ToString();

            if (oldValue == newValue || prop.Name == "UpdatedAt")
                continue;

            var history = new ProjectHistory(
                projectId: projectId,
                relatedEntityId: relatedEntityId,
                relatedEntityTitle: relatedEntityTitle,
                relatedEntityCode: relatedEntityKey,
                scope: scope,
                actionType: ActionType.Update,
                propertyName: prop.Name,
                oldValue: oldValue,
                newValue: newValue,
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
        if (currentUserId == null)
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
        if (currentUserId == null)
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
            nameof(Project) => ProjectHistoryScope.Project,
            nameof(Sprint) => ProjectHistoryScope.Sprint,
            nameof(WorkItem) => ProjectHistoryScope.WorkItem,
            nameof(WorkItemRelation) => ProjectHistoryScope.WorkItemRelation,
            nameof(CodeLink) => ProjectHistoryScope.WorkItemCodeLink,
            nameof(Comment) => ProjectHistoryScope.WorkItemComment,
            nameof(Attachment) => ProjectHistoryScope.WorkItemAttachment,
            _ => throw new ArgumentException($"Unsupported type: {typeof(T).Name}")
        };
    }
}