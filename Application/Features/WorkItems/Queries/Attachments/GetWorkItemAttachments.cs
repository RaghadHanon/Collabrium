using Application.Common;
using Application.Features.WorkItems.DTOs.Attachment;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries.Attachments;
public class GetWorkItemAttachments
{
    public class Query : IRequest<Result<List<AttachmentDto>>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Query, Result<List<AttachmentDto>>>
    {
        public async Task<Result<List<AttachmentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<AttachmentDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<List<AttachmentDto>>.Failure("Project not found.", HttpStatusCode.NotFound);

            var isExist = await workItemsRepository.IsExistAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            if (!isExist)
                return Result<List<AttachmentDto>>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var dtoList = await workItemsReadModel.GetWorkItemAttachments(request.WorkItemId, cancellationToken);

            return Result<List<AttachmentDto>>.Success(dtoList, HttpStatusCode.OK);
        }
    }
}