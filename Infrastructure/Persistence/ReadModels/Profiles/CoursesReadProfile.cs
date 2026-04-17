using Application.Features.Courses.DTOs;
using Application.Features.Tags.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class CoursesReadProfile : Profile
    {
        public CoursesReadProfile()
        {
            CreateMap<TagEntity, TagDto>();
            CreateMap<CourseEntity, CourseDto>()
                 .ForMember(d => d.Tags,
                    o => o.MapFrom(s => s.Tags))
                 .ForMember(d => d.ThumbnailImageUrl,
                     o => o.MapFrom(s => s.ThumbnailImage != null ? s.ThumbnailImage.Url : null))
                 .ForMember(d => d.ThumbnailImagePublicId,
                     o => o.MapFrom(s => s.ThumbnailImage != null ? s.ThumbnailImage.PublicId : null))
                 .ForMember(d => d.RatingsCount,
                     o => o.MapFrom(s => s.CourseRatings.Count))
                 .ForMember(d => d.AverageRating,
                     o => o.MapFrom(s => s.CourseRatings.Any()
                         ? s.CourseRatings.Average(r => (double?)r.Rating) ?? 0
                         : 0));

            CreateMap<CourseEntity, CourseDetailsDto>()
                .IncludeBase<CourseEntity, CourseDto>();
            CreateMap<CourseEntity, CourseDetailsDto>()
           .IncludeBase<CourseEntity, CourseDto>()

            .ForMember(d => d.LearningPoints,
            o => o.MapFrom(s => s.LearningPoints.Select(lp => lp.Text)));
        }
    }
}