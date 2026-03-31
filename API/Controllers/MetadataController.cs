using API.Common.Attributes;
using Application.Features.Metadata.DTOs;
using Application.Features.Metadata.Queries;
using Application.Features.WorkItems.DTOs.Relations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/metadata")]
public sealed class MetadataController : ApiBaseController
{
    [IgnoreTenantContext]
    [HttpGet("user-roles")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetUserRoleMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.UserRole,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.WorkItemType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.WorkItemStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-relation-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemRelationTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.WorkItemRelationType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-priority")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemPriorityMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.WorkItemPriority,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("test-environment")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetTestEnvironmentMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.TestEnvironment,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("testCase-result")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetTestCaseResultMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.TestCaseResult,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("sprint-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetSprintStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.SprintStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("project-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetProjectStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.ProjectStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("department")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetDepartmentMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.Department,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("codeLink-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetCodeLinkTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.CodeLinkType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("attachment-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetAttachmentTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.AttachmentType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItems-relation-rules")]
    public async Task<ActionResult<List<WorkItemsRelationRulesMetadataDto>>> GetWorkItemsRelationRulesMetadata()
    {
        var query = new GetWorkItemsRelationRulesMetadata.Query();

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItems-workflow")]
    public async Task<ActionResult<List<WorkItemWorkflowDto>>> GetWorkItemWorkflow()
    {
        var query = new GetWorkItemsWorkFlowMetadata.Query();

        return HandleResult(await Mediator.Send(query));
    }
    [IgnoreTenantContext]
    [HttpGet("roadmap-levels")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetRoadmapLevelMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapLevel,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("roadmap-paths")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetRoadmapPathMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapPath,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("roadmap-resource-types")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetRoadmapResourceTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapResourceType,
        };

        return HandleResult(await Mediator.Send(query));
    }

}