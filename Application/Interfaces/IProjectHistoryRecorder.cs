using Application.Features.ProjectsHistories.DTOs;

namespace Application.Interfaces;

public interface IProjectHistoryRecorder
{
    Task<IReadOnlyList<RecordedProjectHistoryChangeDto>>
        RecordUpdatingChangesAsync<T>(
            string projectId,
            string relatedEntityId,
            string relatedEntityTitle,
            string relatedEntityKey,
            T oldObject,
            T newObject,
            CancellationToken cancellationToken = default);

    void RecordCreationChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey);

    void RecordDeletionChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityKey);
}