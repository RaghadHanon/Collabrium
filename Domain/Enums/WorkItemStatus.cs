namespace Domain.Enums;

public enum WorkItemStatus
{
    Backlog = 0,
    ReadyForDev = 1,
    InProgress = 2,
    InReview = 3,
    InTesting = 4,
    ReadyForRelease = 5,
    Done = 6,
}