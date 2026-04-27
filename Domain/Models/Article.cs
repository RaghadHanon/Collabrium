using Domain.Common;

namespace Domain.Models
{
    public sealed class Article : Entity
    {
        public readonly HashSet<string> _tagsIds = new();
        public string TenantId { get; private set; } = null;
        public string Title { get; private set; } = null;
        public string Content { get; private set; } = null;
        public string CreatedByUserId { get; private set; } = null;
        public string? CoverImageUrl { get; private set; }
        public bool IsPublished { get; private set; }
        public IReadOnlyCollection<string> TagIds => _tagsIds;
        public Article() { }
        public Article(
            string tenantId,
            string title,
            string content,
            string createdByUserId,
            string? coverImageUrl = null,
            bool isPublished = true)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
            IsPublished = isPublished;

            Touch();

        }
        public Article(
           string id,
           string tenantId,
           string title,
           string content,
           string createdByUserId,
           string? coverImageUrl = null,
           bool isPublished = true) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
            IsPublished = isPublished;
        }
        public static Article Rehydrate(
           string id,
           DateTime createdAt,
           DateTime? updatedAt,
           string tenantId,
           string title,
           string content,
           string createdByUserId,
           string? coverImageUrl,
           bool isPublished,
           IEnumerable<string>? tagIds)
        {
            var article = new Article(
                id: id,
                tenantId: tenantId,
                title: title,
                content: content,
                createdByUserId: createdByUserId,
                coverImageUrl: coverImageUrl,
                isPublished: isPublished);

            article.SetTimestamps(createdAt, updatedAt);

            article._tagsIds.Clear();

            if (tagIds is not null)
            {
                foreach (var tagId in tagIds)
                {
                    if (string.IsNullOrWhiteSpace(tagId))
                        continue;

                    article._tagsIds.Add(tagId.Trim());
                }
            }

            return article;
        }
        public void UpdateDetails(
           string title,
           string content,
           string? coverImageUrl = null,
           bool? isPublished = null)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();

            if (isPublished.HasValue)
                IsPublished = isPublished.Value;

            Touch();
        }

        public void Publish()
        {
            if (IsPublished)
                return;

            IsPublished = true;
            Touch();
        }

        public void UnPublish()
        {
            if (!IsPublished)
                return;

            IsPublished = false;
            Touch();
        }

        public void SetCoverImage(string? coverImageUrl)
        {
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
            Touch();
        }
        public void AddTag(string tagId)
        {
            var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();
            if (_tagsIds.Add(clean))
                Touch();
        }
        public void RemoveTag(string tagId)
        {
            var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

            if (_tagsIds.Remove(clean))
                Touch();
        }
        public void setTags(IEnumerable<string> tagIds)
        {
            _tagsIds.Clear();
            if (tagIds is null)
            {
                Touch();
                return;
            }
            foreach (var tagId in tagIds)
            {
                if (string.IsNullOrWhiteSpace(tagId))
                    continue;
                _tagsIds.Add(tagId.Trim());
            }
            Touch();
        }

    }
}
