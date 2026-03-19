using Domain.Common;

namespace Domain.Models
{
    public sealed class CourseRating : Entity
    {
        public string TenantId { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public string CourseId { get; private set; } = null!;

        public int Rating { get; private set; }
        public string? Review { get; private set; }

        private CourseRating() { }

        public CourseRating(
            string tenantId,
            string userId,
            string courseId,
            int rating,
            string? review = null)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));

            SetRating(rating);
            Review = review?.Trim();
        }

        public CourseRating(
            string id,
            string tenantId,
            string userId,
            string courseId,
            int rating,
            string? review = null) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
            UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));
            CourseId = Guard.NotNullOrWhiteSpace(courseId, nameof(courseId));

            SetRating(rating);
            Review = review?.Trim();
        }

        public static CourseRating Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string userId,
            string courseId,
            int rating,
            string? review)
        {
            var courseRating = new CourseRating(
                id: id,
                tenantId: tenantId,
                userId: userId,
                courseId: courseId,
                rating: rating,
                review: review);

            courseRating.SetTimestamps(createdAt, updatedAt);

            return courseRating;
        }

        public void Update(int rating, string? review)
        {
            SetRating(rating);
            Review = review?.Trim();
            Touch();
        }

        private void SetRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new DomainException("Rating must be between 1 and 5.");

            Rating = rating;
        }
    }
}

