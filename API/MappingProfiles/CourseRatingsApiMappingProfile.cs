using API.Contracts.CourseRatings;
using Application.Features.CourseRatings.Commands;
using Application.Features.CourseRatings.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class CourseRatingsApiMappingProfile : Profile
{
    public CourseRatingsApiMappingProfile()
    {
        CreateMap<RateCourseRequest, RateCourse.Command>();
        CreateMap<GetCourseRatingsRequest, GetCourseRatings.Query>();
    }
}
