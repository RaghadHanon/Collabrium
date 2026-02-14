using API.Contracts.WorkItems;
using API.Contracts.WorkItems.Create;
using API.SetupProgram;
using Application.Features.WorkItems.Commands;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Features.WorkItems.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/work-items")]
[Authorize(Policy = Policies.PasswordChanged)]
[Authorize(Policy = Policies.Project_Access)]
public sealed class WorkItemsRelationsController : ApiBaseController
{
    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPost("{workItemId}/relation")]
    public async Task<ActionResult<WorkItemDto>> AddWorkItemsRelationAssignee(
       string projectId,
       string workItemId,
       AddWorkItemsRelationRequest request)
    {
        var command = Mapper.Map<AddWorkItemsRelation.Command>(request);
        command.ProjectId = projectId;
        command.SourceWorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/relations")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemRelationsAssignee(
      string projectId,
      string workItemId,
      [FromQuery] GetWorkItemRelationsRequest request)
    {
        var command = Mapper.Map<GetWorkItemRelations.Command>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }
}