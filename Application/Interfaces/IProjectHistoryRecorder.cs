namespace Application.Interfaces;

public interface IProjectHistoryRecorder
{
    void RecordUpdatingChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        T oldObject,
        T newObject);

    void RecordCreationChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle);

    void RecordDeletionChanges<T>(
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle);
}