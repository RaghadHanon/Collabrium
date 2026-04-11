using Application.Features.UserCourses.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class UserCoursesReadProfile : Profile
    {
        public UserCoursesReadProfile()
        {
            CreateMap<UserCourseEntity, UserCourseDto>()
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

            CreateMap<UserCourseEntity, UserCourseDetailsDto>()
                .ForMember(d => d.CourseTitle,
                    o => o.MapFrom(s => s.Course.Title))
                .ForMember(d => d.CourseDescription,
                    o => o.MapFrom(s => s.Course.Description))
                .ForMember(d => d.CourseUrl,
                    o => o.MapFrom(s => s.Course.Url))
                .ForMember(d => d.Provider,
                    o => o.MapFrom(s => s.Course.Provider))
                .ForMember(d => d.InstructorName,
                    o => o.MapFrom(s => s.Course.InstructorName))
                .ForMember(d => d.CourseThumbnailUrl,
                    o => o.MapFrom(s => s.Course.ThumbnailImage != null
                        ? s.Course.ThumbnailImage.Url
                        : null))
                .ForMember(d => d.AssignedByUserName,
                    o => o.MapFrom(s => s.AssignedByUser != null
                        ? s.AssignedByUser.FullName
                        : null));


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
}
