using API.Contracts.Events;
using Application.Features.Events.Commands;
using Application.Features.Events.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class EventsRequestProfile : Profile
{
    public EventsRequestProfile()
    {
        CreateMap<GetEventsRequest, GetEvents.Query>();

        CreateMap<GetManageEventsRequest, GetManageEvents.Query>();

        CreateMap<CreateEventRequest, CreateEvent.Command>();

        CreateMap<UpdateEventRequest, UpdateEvent.Command>();
    }
}