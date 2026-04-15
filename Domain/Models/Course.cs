using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Models
{
    public sealed class Course : Entity
    {
        private readonly List<string> _tagIds = new();
        private readonly List<CourseLearningPoint> _learningPoints = new();

        public string TenantId { get; private set; } = null!;

        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string Url { get; private set; } = null!;
        public string Provider { get; private set; } = null!;
        public string? InstructorName { get; private set; }

        public CourseLevel Level { get; private set; }
        public CourseLanguage Language { get; private set; }

        public int? EstimatedDurationInHours { get; private set; }

        public StoredImage? ThumbnailImage { get; private set; }

        public IReadOnlyCollection<string> TagIds => _tagIds.AsReadOnly();
        public IReadOnlyCollection<CourseLearningPoint> LearningPoints => _learningPoints.AsReadOnly();

        private Course() { }

        public Course(
            string tenantId,
            string title,
            string description,
            string url,
            string provider,
            CourseLevel level,
            CourseLanguage language,
            string? instructorName = null,
            int? estimatedDurationInHours = null)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Description = Guard.NotNullOrWhiteSpace(description, nameof(description));
            Url = Guard.NotNullOrWhiteSpace(url, nameof(url));
            Provider = Guard.NotNullOrWhiteSpace(provider, nameof(provider));
            Level = level;
            Language = language;
            InstructorName = instructorName?.Trim();
            EstimatedDurationInHours = estimatedDurationInHours;
        }

        public Course(
            string id,
            string tenantId,
            string title,
            string description,
            string url,
            string provider,
            CourseLevel level,
            CourseLanguage language,
            string? instructorName = null,
            int? estimatedDurationInHours = null) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Description = Guard.NotNullOrWhiteSpace(description, nameof(description));
            Url = Guard.NotNullOrWhiteSpace(url, nameof(url));
            Provider = Guard.NotNullOrWhiteSpace(provider, nameof(provider));
            Level = level;
            Language = language;
            InstructorName = instructorName?.Trim();
            EstimatedDurationInHours = estimatedDurationInHours;
        }

        public static Course Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string title,
            string description,
            string url,
            string provider,
            CourseLevel level,
            CourseLanguage language,
            string? instructorName,
            int? estimatedDurationInHours,
            StoredImage? thumbnailImage,
            IEnumerable<string>? tagIds,
            IEnumerable<CourseLearningPoint>? learningPoints)
        {
            var course = new Course(
                id: id,
                tenantId: tenantId,
                title: title,
                description: description,
                url: url,
                provider: provider,
                level: level,
                language: language,
                instructorName: instructorName,
                estimatedDurationInHours: estimatedDurationInHours);

            course.SetTimestamps(createdAt, updatedAt);
            course.ThumbnailImage = thumbnailImage;

            if (tagIds is not null)
            {
                course._tagIds.Clear();

                foreach (var tagId in tagIds)
                {
                    if (!string.IsNullOrWhiteSpace(tagId))
                        course._tagIds.Add(tagId.Trim());
                }
            }
            course._learningPoints.Clear();
            if (learningPoints is not null)
            {
                foreach (var point in learningPoints)
                    course._learningPoints.Add(point);
            }

            return course;
        }

        public void UpdateDetails(
            string title,
            string description,
            string url,
            string provider,
            CourseLevel level,
            CourseLanguage language,
            string? instructorName,
            int? estimatedDurationInHours)
        {
            Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
            Description = Guard.NotNullOrWhiteSpace(description, nameof(description));
            Url = Guard.NotNullOrWhiteSpace(url, nameof(url));
            Provider = Guard.NotNullOrWhiteSpace(provider, nameof(provider));
            Level = level;
            Language = language;
            InstructorName = instructorName?.Trim();
            EstimatedDurationInHours = estimatedDurationInHours;

            Touch();
        }

        public void SetThumbnailImage(string url, string publicId)
        {
            var image = StoredImage.Create(url, publicId);
            ThumbnailImage = image ?? throw new DomainException("Image is required.");
            Touch();
        }

        public void RemoveThumbnailImage()
        {
            ThumbnailImage = null;
            Touch();
        }
        public void SetLearningPoints(IEnumerable<string>? points)
        {
            _learningPoints.Clear();

            if (points is null)
            {
                Touch();
                return;
            }

            foreach (var point in points)
            {
                if (string.IsNullOrWhiteSpace(point))
                    continue;

                var vo = new CourseLearningPoint(point);

                if (_learningPoints.Any(x => x.Text == vo.Text))
                    continue;

                _learningPoints.Add(vo);
            }

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

                var cleanedTagId = tagId.Trim();

                if (_tagIds.Contains(cleanedTagId))
                    continue;

                _tagIds.Add(cleanedTagId);
            }

            Touch();
        }

        public void AddTag(string tagId)
        {
            var cleanedTagId = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

            if (_tagIds.Contains(cleanedTagId))
                return;

            _tagIds.Add(cleanedTagId);
            Touch();
        }

        public void RemoveTag(string tagId)
        {
            var cleanedTagId = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

            if (_tagIds.Remove(cleanedTagId))
                Touch();
        }
    }
}

