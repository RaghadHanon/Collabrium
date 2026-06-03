namespace Domain.Enums;

public enum NotificationType
{
    System = 0,

    TeamMemberAdded = 1,
    TeamCreated = 2,
    TeamUpdated = 3,
    TeamMemberRemoved = 4,

    ProjectAssigned = 10,
    ProjectUnassigned = 11,

    WorkItemAssigned = 20,
    WorkItemUpdated = 21,
    WorkItemStatusChanged = 22,
    WorkItemCommentAdded = 23,
    WorkItemMention = 24,

    SprintCreated = 30,
    SprintUpdated = 31,
    SprintDeadlineReminder = 32,
    SprintStatusChanged = 33,

    AnnouncementCreated = 40,
    EventCreated = 41,
    CourseAssigned = 50
}