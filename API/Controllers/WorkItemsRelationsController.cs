using API.Contracts.WorkItems;
using API.Contracts.WorkItems.Create;
using API.SetupProgram;
using Application.Features.WorkItems.Commands.Relations;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Features.WorkItems.Queries.Relations;
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
    public async Task<ActionResult<WorkItemDto>> AddWorkItemRelation(
       string projectId,
       string workItemId,
       AddWorkItemRelationRequest request)
    {
        var command = Mapper.Map<AddWorkItemRelation.Command>(request);
        command.ProjectId = projectId;
        command.SourceWorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/relations")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemRelations(
      string projectId,
      string workItemId,
      [FromQuery] GetWorkItemRelationsRequest request)
    {
        var command = Mapper.Map<GetWorkItemRelations.Query>(request);
        command.ProjectId = projectId;
        command.WorkItemId = workItemId;

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.WorkItems_Read)]
    [HttpGet("{workItemId}/relation/{relationId}")]
    public async Task<ActionResult<WorkItemRelationDto>> GetWorkItemRelation(
        string projectId,
        string workItemId,
        string relationId)
    {
        var query = new GetWorkItemRelation.Query 
        {
            ProjectId = projectId, 
            RelationId = relationId, 
            WorkItemId = workItemId 
        };

        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpPatch("{workItemId}/relation/{relationId}")]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItemRelation(
       string projectId,
       string workItemId,
       string relationId,
       UpdateWorkItemRelationRequest request)
    {
        var command = Mapper.Map<UpdateWorkItemRelation.Command>(request);
        command.ProjectId = projectId;
        command.SourceWorkItemId = workItemId;
        command.RelationId = relationId; 

        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Policy = Policies.WorkItems_UpdateContent)]
    [HttpDelete("{workItemId}/relation/{relationId}")]
    public async Task<ActionResult<WorkItemDto>> DeleteWorkItemRelation(
      string projectId,
      string workItemId,
      string relationId)
    {
        var command = new DeleteWorkItemRelation.Command{
            ProjectId = projectId,
            SourceWorkItemId = workItemId,
            RelationId = relationId 
        };

        return HandleResult(await Mediator.Send(command));
    }
}