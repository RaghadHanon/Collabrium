using Domain.Core;

namespace Domain.Models;

public sealed class RoadmapSubTopic : Entity
{
    public string TopicNodeId { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public int Order { get; private set; }

    private RoadmapSubTopic() { } // EF

    public RoadmapSubTopic(
        string topicNodeId,
        string title,
        int order,
        string? description = null)
    {
        TopicNodeId = Guard.NotNullOrWhiteSpace(topicNodeId, nameof(topicNodeId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Description = description?.Trim();
        SetOrder(order);

        Touch();
    }

    public RoadmapSubTopic(
        string id,
        string topicNodeId,
        string title,
        int order,
        string? description = null)
        : base(id)
    {
        TopicNodeId = Guard.NotNullOrWhiteSpace(topicNodeId, nameof(topicNodeId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Description = description?.Trim();
        SetOrder(order);
    }

    public static RoadmapSubTopic Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string topicNodeId,
        string title,
        string? description,
        int order)
    {
        var s = new RoadmapSubTopic(id, topicNodeId, title, order, description);
        s.SetTimestamps(createdAt, updatedAt);
        return s;
    }

    public void UpdateDetails(string title, string? description)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Description = description?.Trim();
        Touch();
    }

    public void ChangeOrder(int order)
    {
        SetOrder(order);
        Touch();
    }

    private void SetOrder(int order)
    {
        Guard.Against(order < 1, "Order must be >= 1.");
        Order = order;
    }
}
