namespace Application.Features.ProjectsHistories.DTOs;

public sealed record RecordedProjectHistoryChangeDto(
string PropertyName,
string? OldValue,
string? NewValue
);