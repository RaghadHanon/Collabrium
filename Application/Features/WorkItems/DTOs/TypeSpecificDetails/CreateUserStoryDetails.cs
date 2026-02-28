using Application.Interfaces;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public class CreateUserStoryDetails : IWorkItemCreateDetails
{
    public required int StoryPoints { get; init; }
}
