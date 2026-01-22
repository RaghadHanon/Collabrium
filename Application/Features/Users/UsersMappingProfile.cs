using Application.Features.Tenants.DTOs;
using Application.Features.Tenants.Queries;
using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Users;
public class UsersMappingProfile : Profile
{
    public UsersMappingProfile()
    {
        CreateMap<ApplicationUser, UserWithTenantDto>();

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.ProfileImageUrl, opt => opt.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null))
            .ForMember(d => d.ProfileImageUrl, opt => opt.MapFrom(s => s.ProfileImage != null? s.ProfileImage.Url : null));
        
        CreateMap<ApplicationUser, TenantAdmin>();
        CreateMap<GetTenantUsers.Query, UsersSearchAndFilterParametersDto>();
        CreateMap<UserContactInfo, UserContactInfoDto>();

        CreateMap<UpdateUserProfile.Command, ApplicationUser>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UpdateUserProfile.Command, UserContactInfo>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}