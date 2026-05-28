using API.Contracts.Tenants;
using API.Contracts.Users;
using Application.Features.Users.Commands;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class UsersApiMappingProfile : Profile
{
    public UsersApiMappingProfile()
    {
        CreateMap<UpdateUserProfileRequest, UpdateUserProfile.Command>();
        CreateMap<UpdateUserRoleRequest, UpdateUserRole.Command>();
        CreateMap<UploadImageRequest, UploadUserProfileImage.Command>();
        CreateMap<AddUserSkillsRequest, AddUserSkills.Command>(); 
        CreateMap<DeleteUserSkillsRequest, DeleteUserSkills.Command>();
    }
}