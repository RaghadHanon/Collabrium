namespace Domain.Models
{
    public sealed class TopicNode : RoadmapNode
    {
        private TopicNode() { } // EF

        public TopicNode(
            string tenantId,
            string roadmapId,
            string title,
            int order,
            string? description = null)
            : base(tenantId, roadmapId, title, order, description)
        { }

        public TopicNode(
            string id,
            string tenantId,
            string roadmapId,
            string title,
            int order,
            string? description = null)
            : base(id, tenantId, roadmapId, title, order, description)
        { }

        public static TopicNode Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string roadmapId,
            string title,
            string? description,
            int order)
        {
            var n = new TopicNode(id, tenantId, roadmapId, title, order, description);
            n.SetTimestamps(createdAt, updatedAt);
            return n;
        }
    }
}
