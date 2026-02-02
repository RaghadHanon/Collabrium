using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class UserStory : WorkItem
{
    public int StoryPoints { get; private set; }

    private UserStory() { }

    public UserStory(string id, string title, string createdByUserId, string projectId, int storyPoints)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.UserStory;
        Estimate(storyPoints);
    }

    public static UserStory Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string title,
        string createdByUserId,
        string projectId,
        WorkItemStatus status,
        WorkItemPriority priority,
        bool isBlocked,
        int effortPoints,
        string? description,
        string? acceptanceCriteria,
        string? assignedToUserId,
        string? sprintId,
        DateTime? startDate,
        DateTime? endDate,
        int storyPoints)
    {
        var us = new UserStory(id, title, createdByUserId, projectId, storyPoints);
        us.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        us.StoryPoints = storyPoints;
        return us;
    }

    public void Estimate(int points)
    {
        Guard.Against(points < 0, "StoryPoints cannot be negative.");
        Guard.Against(points > 100, "StoryPoints is too high.");
        StoryPoints = points;
        Touch();
    }
}