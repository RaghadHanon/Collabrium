using Domain.Common;

namespace Domain.Models
{
    public abstract class RoadmapNode : Entity
    {
        public string RoadmapId { get; protected set; } = null!;

        public string Title { get; protected set; } = null!;
        public string? Description { get; protected set; }

        public int Order { get; protected set; }

        protected RoadmapNode() { } // EF

        protected RoadmapNode(
            string roadmapId,
            string title,
            int order,
            string? description = null)
        {
            RoadmapId = Guard.NotNullOrWhiteSpace(roadmapId, nameof(roadmapId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Description = description?.Trim();
            SetOrder(order);

            Touch();
        }

        protected RoadmapNode(
            string id,
            string roadmapId,
            string title,
            int order,
            string? description = null)
            : base(id)
        {
            RoadmapId = Guard.NotNullOrWhiteSpace(roadmapId, nameof(roadmapId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Description = description?.Trim();
            SetOrder(order);
        }

        public void Update(string title, string? description, int order)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Description = description?.Trim();
            SetOrder(order);

            Touch();
        }

        public void ChangeOrder(int order)
        {
            SetOrder(order);
            Touch();
        }

        protected void SetOrder(int order)
        {
            Guard.Against(order < 1, "Order must be >= 1.");
            Order = order;
        }
    }
}
