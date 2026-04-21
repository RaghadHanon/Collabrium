using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record CourseLearningPoint
    {
        public string Text { get; }

        public CourseLearningPoint(string text)
        {
            Text = Guard.NotNullOrWhiteSpace(text, nameof(text)).Trim();

            if (Text.Length > 200)
                throw new DomainException("Learning point cannot exceed 200 characters.");
        }
    }
}
