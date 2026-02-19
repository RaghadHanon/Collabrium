using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.CodeLinks;

public class AddWorkItemCodeLink
{
    public class Command : IRequest<Result<string>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string Url { get; set; }
        public CodeLinkType LinkType { get; set; }
        public string? DisplayName { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<string>.Failure("Project not found.", HttpStatusCode.NotFound);

            var isExist = await workItemsRepository.IsExistAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            if (!isExist)
                return Result<string>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<string>.Failure("Current User not found", HttpStatusCode.NotFound);


            CodeLink? codeLink = null;
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                codeLink = new CodeLink(request.Url, request.LinkType, currentUserId, request.DisplayName);

                await workItemsRepository.AddCodeLinkOnWorkItemAsync(
                    tenantId,
                    request.ProjectId,
                    request.WorkItemId,
                    codeLink,
                    cancellationToken);
            }, cancellationToken);

            return Result<string>.Success(codeLink?.Id, HttpStatusCode.OK);
        }
    }
}