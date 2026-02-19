using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Comments;

public class UpdateWorkItemCodeLink
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string CodeLinkId { get; set; }
        public string? Url { get; set; }
        public CodeLinkType? LinkType { get; set; }
        public string? DisplayName { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var isWorkItemExist = await workItemsRepository.IsExistAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            if (!isWorkItemExist)
                return Result<Unit>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<Unit>.Failure("Current User not found.", HttpStatusCode.NotFound);

            var codeLink = await workItemsRepository.GetCodeLinkOnWorkItemByIdAsync(request.WorkItemId, request.CodeLinkId, cancellationToken);
            if (codeLink == null)
                return Result<Unit>.Failure("CodeLink not found.", HttpStatusCode.NotFound);

            if(currentUserId != codeLink.CreatedByUserId)
                return Result<Unit>.Failure("You don't own this codeLink to update it.", HttpStatusCode.Forbidden);

            var UrlDesision = PatchHelpers.DecideStringPatch(request.Url);
            if (UrlDesision.Action == PatchHelpers.PatchStringAction.Clear)
                return Result<Unit>.Failure("Url can't be empty.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                codeLink.Update(
                    url: PatchHelpers.ApplyStringPatch(codeLink.Url, request.Url)!,
                    linkType: request.LinkType ?? codeLink.LinkType,
                    displayName: PatchHelpers.ApplyStringPatch(codeLink.DisplayName, request.DisplayName));

                await workItemsRepository.UpdateCodeLinkOfWorkItemAsync(
                    request.WorkItemId,
                    codeLink,
                    cancellationToken);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}