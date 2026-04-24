using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Models
{
    public sealed class Article : Entity
    {
        private readonly HashSet<string> _tagIds = new();
        public string TeanantId { get; private set; } = null!;
        public string Title { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public string CreatedByUserId { get; private set; } = null!;
        public StoredImage? CoverImage { get; private set; }
        public bool IsPublished { get; private set; }
        public IReadOnlyCollection<string> TagIds => _tagIds;
        private Article() { }
        public Article(
            string tenantId,
            string title,
            string content,
            string createdByUserId,
            bool isPublished = true)
        {
            TeanantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
            this.IsPublished = isPublished;
            Touch();
        }

        public Article(
                string id,
                string tenantId,
                string title,
                string content,
                string createdByUserId,
                bool isPublished = true) : base(id)
        {
            TeanantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
            this.IsPublished = isPublished;
        }
        public static Article Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string title,
            string content,
            string createdByUserId,
            StoredImage? coverImage,
            bool isPublished,
            IEnumerable<string>? tagIds)
        {
            var article = new Article(
                id: id,
                tenantId: tenantId,
                title: title,
                content: content,
                createdByUserId: createdByUserId,
                isPublished: isPublished);
            article.SetTimestamps(createdAt, updatedAt);
            article.CoverImage = coverImage;
            article._tagIds.Clear();
            if (tagIds is not null)
            {
                foreach (var tagId in tagIds)
                {
                    article._tagIds.Add(tagId);
                }
            }
            return article;
        }
        public void UpdateDetails(
            string title,
            string content,
            bool? isPublished = null)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
            if (isPublished.HasValue)
                this.IsPublished = isPublished.Value;
            Touch();
        }
        public void SetCoverImage(string url, string publicId)
        {
            var image = StoredImage.Create(url, publicId);
            CoverImage = image ?? throw new DomainException("Image is required.");
            Touch();
        }
        public void RemoveCoverImage()
        {
            CoverImage = null;
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
        public void AddTag(string tagId)
        {
            var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId));
            if (_tagIds.Contains(clean))
                Touch();
        }
        public void RemoveTag(string tagId)
        {
            var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

            if (_tagIds.Remove(clean))
                Touch();
        }
        public void SetTags(IEnumerable<string>? tagIds)
        {
            _tagIds.Clear();

            if (tagIds is null)
            {
                Touch();
                return;
            }

            foreach (var tagId in tagIds)
            {
                if (string.IsNullOrWhiteSpace(tagId))
                    continue;

                _tagIds.Add(tagId.Trim());
            }

            Touch();
        }
    }
}
