using Domain.Enums;

namespace Domain.Models;

public sealed class UserStory : WorkItem
{
    public int StoryPoints { get; private set; }

    public UserStory(string id, string title, string createdByUserId, string projectId, int storyPoints)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.UserStory;
        StoryPoints = storyPoints;
    }

    public void Estimate(int points)
    {
        StoryPoints = points;
    }
}
