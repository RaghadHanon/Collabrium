namespace Application.Interfaces;

public interface IProjectHistoryValueResolver
{
    Task<string?> ResolveAsync(
        string tenantId,
        string projectId,
        string propertyName,
        string? value,
        CancellationToken cancellationToken = default);
}