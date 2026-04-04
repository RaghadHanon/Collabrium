using API.Contracts.UserCourses;
using Application.Features.UserCourses.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public sealed class UserCoursesApiMappingProfile : Profile
    {
        public UserCoursesApiMappingProfile()
        {
            CreateMap<GetMyUserCoursesRequest, GetMyUserCourses.Query>();
            CreateMap<GetMySavedUserCoursesRequest, GetMySavedUserCourses.Query>();
        }
    }
}
