using Application.Features.Tags.DTOs;
using Application.Features.UserCourses.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class UserCoursesReadProfile : Profile
{
    public UserCoursesReadProfile()
    {
        CreateMap<TagEntity, TagDto>();

        CreateMap<UserCourseEntity, UserCourseDto>()
            .ForMember(d => d.Id,
                o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseId,
                o => o.MapFrom(s => s.CourseId))
            .ForMember(d => d.TenantId,
                o => o.MapFrom(s => s.TenantId))
            .ForMember(d => d.UserId,
                o => o.MapFrom(s => s.UserId))

            .ForMember(d => d.Title,
                o => o.MapFrom(s => s.Course.Title))
            .ForMember(d => d.Description,
                o => o.MapFrom(s => s.Course.Description))
            .ForMember(d => d.Url,
                o => o.MapFrom(s => s.Course.Url))
            .ForMember(d => d.Provider,
                o => o.MapFrom(s => s.Course.Provider))
            .ForMember(d => d.InstructorName,
                o => o.MapFrom(s => s.Course.InstructorName))
            .ForMember(d => d.Level,
                o => o.MapFrom(s => s.Course.Level))
            .ForMember(d => d.Language,
                o => o.MapFrom(s => s.Course.Language))
            .ForMember(d => d.EstimatedDurationInHours,
                o => o.MapFrom(s => s.Course.EstimatedDurationInHours))
            .ForMember(d => d.Tags,
                o => o.MapFrom(s => s.Course.Tags))
            .ForMember(d => d.ThumbnailImageUrl,
                o => o.MapFrom(s => s.Course.ThumbnailImage != null
                    ? s.Course.ThumbnailImage.Url
                    : null))
            .ForMember(d => d.ThumbnailImagePublicId,
                o => o.MapFrom(s => s.Course.ThumbnailImage != null
                    ? s.Course.ThumbnailImage.PublicId
                    : null))
            .ForMember(d => d.RatingsCount,
                o => o.MapFrom(s => s.Course.CourseRatings.Count))
            .ForMember(d => d.AverageRating,
                o => o.MapFrom(s => s.Course.CourseRatings.Any()
                    ? s.Course.CourseRatings.Average(r => (double?) r.Rating) ?? 0
                    : 0))
            .ForMember(d => d.CreatedAt,
                o => o.MapFrom(s => s.Course.CreatedAt))
            .ForMember(d => d.UpdatedAt,
                o => o.MapFrom(s => s.Course.UpdatedAt))
            .ForMember(d => d.AssignedByUserName,
                o => o.MapFrom(s => s.AssignedByUser != null
                    ? s.AssignedByUser.FullName
                    : null));

        CreateMap<UserCourseEntity, UserCourseDetailsDto>()
            .IncludeBase<UserCourseEntity, UserCourseDto>();

        CreateMap<UserCourseEntity, AssignedUserCourseDto>()
            .ForMember(d => d.UserName,
                o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.CourseTitle,
                o => o.MapFrom(s => s.Course.Title))
            .ForMember(d => d.CourseThumbnailUrl,
                o => o.MapFrom(s => s.Course.ThumbnailImage != null
                    ? s.Course.ThumbnailImage.Url
                    : null))
            .ForMember(d => d.AssignedByUserName,
                o => o.MapFrom(s => s.AssignedByUser != null
                    ? s.AssignedByUser.FullName
                    : null));
    }
}


