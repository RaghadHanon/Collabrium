using Domain.Models;
using Domain.ValueObjects;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class CourseMapper
{
    public static CourseEntity ToEntity(Course domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new CourseEntity
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            Title = domain.Title,
            Description = domain.Description,
            Url = domain.Url,
            Provider = domain.Provider,
            InstructorName = domain.InstructorName,
            Level = domain.Level,
            Language = domain.Language,
            EstimatedDurationInHours = domain.EstimatedDurationInHours,
            ThumbnailImage = domain.ThumbnailImage,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,
            LearningPoints = domain.LearningPoints
                .Select(x => new CourseLearningPointEntity
                {
                    Text = x.Text
                })
                .ToList()
        };
    }

    public static Course ToDomain(CourseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var tagIds = entity.Tags?
            .Select(x => x.Id) ?? Enumerable.Empty<string>();

        return Course.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            title: entity.Title,
            description: entity.Description,
            url: entity.Url,
            provider: entity.Provider,
            level: entity.Level,
            language: entity.Language,
            instructorName: entity.InstructorName,
            estimatedDurationInHours: entity.EstimatedDurationInHours,
            thumbnailImage: entity.ThumbnailImage,
            tagIds: tagIds,
            learningPoints: entity.LearningPoints?.Select(x => new CourseLearningPoint(x.Text))
        );
    }

    public static void UpdateEntity(CourseEntity entity, Course domain)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(domain);

        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.Url = domain.Url;
        entity.Provider = domain.Provider;
        entity.InstructorName = domain.InstructorName;
        entity.Level = domain.Level;
        entity.Language = domain.Language;
        entity.EstimatedDurationInHours = domain.EstimatedDurationInHours;
        entity.ThumbnailImage = domain.ThumbnailImage;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;

        entity.LearningPoints.Clear();

        foreach (var point in domain.LearningPoints)
        {
            entity.LearningPoints.Add(new CourseLearningPointEntity
            {
                Text = point.Text
            });
        }
    }
}
