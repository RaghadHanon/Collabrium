using Application.Features.CourseRatings.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class CourseRatingsReadProfile : Profile
    {
        public CourseRatingsReadProfile()
        {
            CreateMap<CourseRatingEntity, CourseRatingDto>()
                .ForMember(d => d.UserName,
                    o => o.MapFrom(s => s.User != null ? s.User.FullName : null))
                .ForMember(d => d.CourseTitle,
                    o => o.MapFrom(s => s.Course != null ? s.Course.Title : null));

            CreateMap<CourseRatingEntity, CourseRatingDetailsDto>()
                .ForMember(d => d.UserName,
                    o => o.MapFrom(s => s.User != null ? s.User.FullName : null))
                .ForMember(d => d.CourseTitle,
                    o => o.MapFrom(s => s.Course != null ? s.Course.Title : null))
                .ForMember(d => d.CourseDescription,
                    o => o.MapFrom(s => s.Course != null ? s.Course.Description : null))
                .ForMember(d => d.CourseThumbnailUrl,
                    o => o.MapFrom(s =>
                        s.Course != null && s.Course.ThumbnailImage != null
                            ? s.Course.ThumbnailImage.Url
                            : null));
        }
    }
}
