using API.Contracts.Announcements;
using Application.Features.Announcements.Commands;
using Application.Features.Announcements.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class AnnouncementContractsMappingProfile : Profile
{
    public AnnouncementContractsMappingProfile()
    {
        CreateMap<CreateAnnouncementRequest, CreateAnnouncement.Command>();

        CreateMap<UpdateAnnouncementRequest, UpdateAnnouncement.Command>();

        CreateMap<GetAnnouncementsRequest, GetAnnouncements.Query>();

        CreateMap<GetManageAnnouncementsRequest, GetManageAnnouncements.Query>();
    }
}