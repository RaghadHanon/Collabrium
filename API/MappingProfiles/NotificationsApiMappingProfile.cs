using API.Contracts.Notifications;
using Application.Features.Notifications.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class NotificationsApiMappingProfile : Profile
{
    public NotificationsApiMappingProfile()
    {
        CreateMap<GetMyNotificationsRequest, GetMyNotifications.Query>();
    }
}