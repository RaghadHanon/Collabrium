using Application.Common.DTOs;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;
using System.Text.RegularExpressions;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class UsersReadProfile : Profile
{
    public UsersReadProfile()
    {
        CreateMap<ApplicationUserEntity, UserDto>()
            .ForMember(d => d.ProfileImageUrl, o => o.MapFrom(s => s.ProfileImage != null ? s.ProfileImage.Url : null))
            .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.JobTitle.Length > 3 ? Regex.Replace(s.JobTitle, "(\\B[A-Z])", " $1") : s.JobTitle));

        CreateMap<ApplicationUserEntity, TenantAdminDto>();
        CreateMap<ContactDetailsOwned, ContactDetailsDto>();
        CreateMap<SocialLinksOwned, SocialLinksDto>();
        CreateMap<ApplicationUserEntity, UserWithTenantDto>()
            .IncludeBase<ApplicationUserEntity, UserDto>();
        CreateMap<ApplicationUserEntity, UserSnapshotDto>();
        CreateMap<TeamMemberEntity, UserSnapshotDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.User.Id))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email));
    }
}