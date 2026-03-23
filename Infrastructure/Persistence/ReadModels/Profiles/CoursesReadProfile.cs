using Application.Features.Courses.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class CoursesReadProfile : Profile
    {
        public CoursesReadProfile()
        {
            CreateMap<CourseTagEntity, CourseTagDto>()
                .ForMember(d => d.Id,
                    o => o.MapFrom(s => s.Tag.Id))
                .ForMember(d => d.Name,
                    o => o.MapFrom(s => s.Tag.Name));

            CreateMap<CourseEntity, CourseDto>()
                 .ForMember(d => d.TagIds,
                     o => o.MapFrom(s => s.CourseTags.Select(t => t.TagId)))
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
                .ForMember(d => d.TagIds,
                    o => o.MapFrom(s => s.CourseTags.Select(t => t.TagId)))
                .ForMember(d => d.Tags,
                    o => o.MapFrom(s => s.CourseTags))
                .ForMember(d => d.ThumbnailImageUrl,
                    o => o.MapFrom(s => s.ThumbnailImage != null ? s.ThumbnailImage.Url : null))
                .ForMember(d => d.ThumbnailImagePublicId,
                    o => o.MapFrom(s => s.ThumbnailImage != null ? s.ThumbnailImage.PublicId : null))
                .ForMember(d => d.RatingsCount,
                    o => o.MapFrom(s => s.CourseRatings.Count))
                .ForMember(d => d.AverageRating,
                    o => o.MapFrom(s => s.CourseRatings.Any()
                        ? s.CourseRatings.Average(r => (double?)r.Rating) ?? 0
                        : 0))
                .ForMember(d => d.AssignmentsCount,
                    o => o.MapFrom(s => s.CourseAssignments.Count))
                .ForMember(d => d.UserProgressCount,
                    o => o.MapFrom(s => s.UserCourses.Count));
        }
    }
}
