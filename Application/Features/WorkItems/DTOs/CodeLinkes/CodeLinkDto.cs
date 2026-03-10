using Application.Features.WorkItems.DTOs.Comments;
using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.CodeLinkes;
public class CodeLinkDto
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string Url { get; private set; } = null!;
    public CodeLinkType LinkType { get; private set; }
    public string? DisplayName { get; private set; }
    public required RelatedUserDto RelatedUser { get; set; }
}