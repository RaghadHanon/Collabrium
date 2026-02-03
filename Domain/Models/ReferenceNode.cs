using Domain.Common;

namespace Domain.Models
{
    public sealed class ReferenceNode : RoadmapNode
    {
        public string RefRoadmapId { get; private set; } = null!;

        private ReferenceNode() { } // EF

        public ReferenceNode(
            string roadmapId,
            string title,
            int order,
            string refRoadmapId,
            string? description = null)
            : base(roadmapId, title, order, description)
        {
            SetReference(roadmapId, refRoadmapId);
            Touch();
        }

        public ReferenceNode(
            string id,
            string roadmapId,
            string title,
            int order,
            string refRoadmapId,
            string? description = null)
            : base(id, roadmapId, title, order, description)
        {
            SetReference(roadmapId, refRoadmapId);
        }

        public static ReferenceNode Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string roadmapId,
            string title,
            string? description,
            int order,
            string refRoadmapId)
        {
            var n = new ReferenceNode(id, roadmapId, title, order, refRoadmapId, description);
            n.SetTimestamps(createdAt, updatedAt);
            return n;
        }

        public void ChangeReference(string refRoadmapId)
        {
            SetReference(RoadmapId, refRoadmapId);
            Touch();
        }

        private void SetReference(string roadmapId, string refRoadmapId)
        {
            var cleanRef = Guard.NotNullOrWhiteSpace(refRoadmapId, nameof(refRoadmapId)).Trim();

            // Business rule: no self-reference
            if (string.Equals(cleanRef, roadmapId, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("A roadmap cannot reference itself.");

            RefRoadmapId = cleanRef;
        }
    }
}
