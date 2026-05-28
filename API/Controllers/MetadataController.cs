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
    public async Task<ActionResult> GetUserRoleMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.UserRole,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("workItem-type")]
    public async Task<ActionResult> GetWorkItemTypeMetadata()
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
    public async Task<ActionResult> GetWorkItemPriorityMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.WorkItemPriority,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("test-environment")]
    public async Task<ActionResult> GetTestEnvironmentMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.TestEnvironment,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("testCase-result")]
    public async Task<ActionResult> GetTestCaseResultMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.TestCaseResult,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("sprint-status")]
    public async Task<ActionResult> GetSprintStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.SprintStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("project-status")]
    public async Task<ActionResult> GetProjectStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.ProjectStatus,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("department")]
    public async Task<ActionResult> GetDepartmentMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.Department,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("codeLink-type")]
    public async Task<ActionResult> GetCodeLinkTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.CodeLinkType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("attachment-type")]
    public async Task<ActionResult> GetAttachmentTypeMetadata()
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
    public async Task<ActionResult> GetWorkItemWorkflow()
    {
        var query = new GetWorkItemsWorkFlowMetadata.Query();

        return HandleResult(await Mediator.Send(query));
    }
    [IgnoreTenantContext]
    [HttpGet("roadmap-levels")]
    public async Task<ActionResult> GetRoadmapLevelMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapLevel,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("roadmap-paths")]
    public async Task<ActionResult> GetRoadmapPathMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapPath,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("roadmap-resource-types")]
    public async Task<ActionResult> GetRoadmapResourceTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.RoadmapResourceType,
        };

        return HandleResult(await Mediator.Send(query));
    }

    [IgnoreTenantContext]
    [HttpGet("course-levels")]
    public async Task<ActionResult> GetCourseLevelMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.CourseLevel,
        };

        return HandleResult(await Mediator.Send(query));

    }
    [IgnoreTenantContext]
    [HttpGet("course-languages")]
    public async Task<ActionResult> GetCourseLanguageMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.CourseLanguage,
        };

        return HandleResult(await Mediator.Send(query));

    }
    //UserCourseStatus
    [IgnoreTenantContext]
    [HttpGet("User-Course-Status")]
    public async Task<ActionResult> GetUserCourseStatusMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.UserCourseStatus,
        };

        return HandleResult(await Mediator.Send(query));

    }
    //PostType
    [IgnoreTenantContext]
    [HttpGet("Post-Type")]
    public async Task<ActionResult> GetPostTypeMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.PostType,
        };

        return HandleResult(await Mediator.Send(query));

    }

    //AnnouncementScopeFilter
    [IgnoreTenantContext]
    [HttpGet("Announcement-Scope-Filter")]
    public async Task<ActionResult> GetAnnouncementScopeFilterMetadata()
    {
        var query = new GetEnumMetaData.Query
        {
            MetadataType = MetadataType.AnnouncementScopeFilter,
        };

        return HandleResult(await Mediator.Send(query));

    }

}

