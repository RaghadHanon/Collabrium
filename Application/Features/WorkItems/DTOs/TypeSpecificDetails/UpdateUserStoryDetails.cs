using Application.Interfaces;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public sealed class UpdateUserStoryDetails : IWorkItemUpdateDetails
{
    public int? StoryPoints { get; init; }
}
