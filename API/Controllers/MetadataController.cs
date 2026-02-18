using API.Common.Attributes;
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
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.UserRole,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemTypeMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.WorkItemType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemStatusMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.WorkItemStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-relation-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemRelationTypeMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.WorkItemRelationType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-priority")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetWorkItemPriorityMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.WorkItemPriority,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("test-environment")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetTestEnvironmentMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.TestEnvironment,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("testCase-result")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetTestCaseResultMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.TestCaseResult,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("sprint-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetSprintStatusMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.SprintStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("project-status")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetProjectStatusMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.ProjectStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("department")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetDepartmentMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.Department,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("codeLink-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetCodeLinkTypeMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.CodeLinkType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("attachment-type")]
    public async Task<ActionResult<List<WorkItemRelationDto>>> GetAttachmentTypeMetadata()
    {
        var query = new GetMetaData.Query
        {
            MetadataType = MetadataType.AttachmentType,
        };

        return HandleResult(await Mediator.Send(query));
    }
}