using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public sealed class Roadmap : Entity
    {
        public RoadmapPath Path { get; private set; }
        public RoadmapLevel Level { get; private set; }

        public string Title { get; private set; } = null!;
        public string? Description { get; private set; }

        public DateTime? PublishedAt { get; private set; }
        public bool IsVisible { get; private set; } = true;

        public string TenantId { get; private set; } = null!;
        public string? CreatedByUserId { get; private set; }

        private readonly HashSet<string> _tagIds = new();
        public IReadOnlyCollection<string> TagIds => _tagIds;

        private Roadmap() { } // EF

        public Roadmap(
            RoadmapPath path,
            RoadmapLevel level,
            string title,
            string tenantId,
            string? description = null,
            string? createdByUserId = null,
            bool IsVisible = true)
        {
            Path = path;
            Level = level;

            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();

            Description = description?.Trim();
            CreatedByUserId = string.IsNullOrWhiteSpace(createdByUserId) ? null : createdByUserId.Trim();
            IsVisible = IsVisible;

            Touch();
        }

        public Roadmap(
            string id,
            RoadmapPath path,
            RoadmapLevel level,
            string title,
            string tenantId,
            string? createdByUserId = null,
            bool isVisible = true)
            : this(path, level, title, tenantId, null, createdByUserId, isVisible)
        {
            Id = id;
        }

        public static Roadmap Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            RoadmapPath path,
            RoadmapLevel level,
            string title,
            string? description,
            DateTime? publishedAt,
            bool IsVisible,
            string tenantId,
            string? createdByUserId,
            IEnumerable<string>? tagIds)
        {
            var r = new Roadmap(id, path, level, title, tenantId, createdByUserId, IsVisible);
            r.SetTimestamps(createdAt, updatedAt);

            r.Description = description?.Trim();
            r.PublishedAt = publishedAt;
            r.IsVisible = IsVisible;

            r._tagIds.Clear();
            if (tagIds is not null)
            {
                foreach (var t in tagIds.Distinct())
                {
                    var clean = Guard.NotNullOrWhiteSpace(t, nameof(tagIds)).Trim();
                    r._tagIds.Add(clean);
                }
            }

            return r;
        }

        public void UpdateDetails(
    string title,
    string? description = null,
    RoadmapLevel? level = null,
    bool? isVisible = null
)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Description = description?.Trim();

            if (level.HasValue)
                Level = level.Value;

            if (isVisible.HasValue)
                IsVisible = isVisible.Value;

            Touch();
        }

        public void ChangePath(RoadmapPath path)
        {
            Path = path;
            Touch();
        }

        // ---------- Tags ----------
        public void AddTag(string tagId)
        {

            tagId = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

            Guard.Against(!_tagIds.Add(tagId), "Tag is already added to this roadmap.");
            Touch();
        }

        public void RemoveTag(string tagId)
        {

            tagId = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();
            Guard.Against(!_tagIds.Remove(tagId), "Tag does not exist on this roadmap.");
            Touch();
        }

        public void SetTags(IEnumerable<string> tagIds)
        {
            Guard.Against(tagIds is null, "Tag list cannot be null.");

            _tagIds.Clear();

            foreach (var id in tagIds.Distinct())
            {
                AddTag(id);
            }

            Touch();
        }

        // ---------- Publish / Active ----------
        public void Publish(DateTime publishedAt)
        {
            Guard.Against(publishedAt < CreatedAt, "PublishedAt cannot be earlier than CreatedAt.");
            PublishedAt = publishedAt;
            Touch();
        }

        public void Show() { IsVisible = true; Touch(); }
        public void Hide() { IsVisible = false; Touch(); }
    }
}