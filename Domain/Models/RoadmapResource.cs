using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public sealed class RoadmapResource : Entity
    {
        public string SubTopicId { get; private set; } = null!;

        public string Title { get; private set; } = null!;
        public string Url { get; private set; } = null!;
        public RoadmapResourceType Type { get; private set; }

        public int Order { get; private set; }

        private RoadmapResource() { } // EF

        public RoadmapResource(
            string subTopicId,
            string title,
            string url,
            RoadmapResourceType type,
            int order)
        {
            SubTopicId = Guard.NotNullOrWhiteSpace(subTopicId, nameof(subTopicId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();

            Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
            Type = type;

            SetOrder(order);

            Touch();
        }

        public RoadmapResource(
            string id,
            string subTopicId,
            string title,
            string url,
            RoadmapResourceType type,
            int order)
            : base(id)
        {
            SubTopicId = Guard.NotNullOrWhiteSpace(subTopicId, nameof(subTopicId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();

            Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
            Type = type;

            SetOrder(order);
        }

        public static RoadmapResource Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string subTopicId,
            string title,
            string url,
            RoadmapResourceType type,
            int order)
        {
            var r = new RoadmapResource(id, subTopicId, title, url, type, order);
            r.SetTimestamps(createdAt, updatedAt);
            return r;
        }

        public void Update(string title, string url, RoadmapResourceType type, int order)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
            Type = type;
            SetOrder(order);

            Touch();
        }

        private void SetOrder(int order)
        {
            Guard.Against(order < 1, "Order must be >= 1.");
            Order = order;
        }
    }
}
