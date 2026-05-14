using API.Contracts.Courses;
using Application.Features.Courses.Commands;
using Application.Features.Courses.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class CoursesApiMappingProfile : Profile
{
    public CoursesApiMappingProfile()
    {
        CreateMap<CreateCourseRequest, CreateCourse.Command>();
        CreateMap<UpdateCourseRequest, UpdateCourse.Command>();
        CreateMap<GetCoursesRequest, GetCourses.Query>();
        CreateMap<UploadCourseThumbnailRequest, UploadCourseThumbnail.Command>();
        CreateMap<AddCourseTagsRequest, AddCourseTags.Command>();
        CreateMap<RemoveCourseTagsRequest, RemoveCourseTags.Command>();
        CreateMap<SetCourseTagsRequest, SetCourseTags.Command>();

    }
}
