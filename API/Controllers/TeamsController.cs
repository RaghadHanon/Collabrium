using API.Contracts.Teams;
using API.SetupProgram;
using Application.Common;
using Application.Features.Teams.Commands;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/teams")]
[Authorize(Policy = Policies.PasswordChanged)]
public class TeamsController : ApiBaseController
{
    /// <summary>
    /// Creates a new team within the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Tenant Admins and allows them to create and manage teams
    /// inside their own tenant.
    ///
    /// Sample request:
    ///
    ///     POST /api/teams/team
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "teamLeadId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "name": "Unity Team",
    ///       "description": "Responsible for Unity project development",
    ///       "department": "SoftwareDevelopment",
    ///       "capacity": 8
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The team creation details including name, description, department, capacity, and team lead.</param>
    /// <returns>The created team with its tenant information.</returns>
    /// <response code="201">Returns the newly created team successfully.</response>
    /// <response code="400">If a team with the same name already exists in the tenant.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Tenant Admin.</response>
    /// <response code="404">If the tenant does not exist.</response>
    [Authorize(Policy = Policies.Teams_Manage)]
    [HttpPost("team")]
    public async Task<ActionResult<string>> CreateTeam(CreateTeamRequest request)
    {
        var command = Mapper.Map<CreateTeam.Command>(request);

        return HandleResult(await Mediator.Send(command), "GetTeam", "Teams");
    }

    /// <summary>
    /// Retrieves detailed information about a specific team.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the team details including its members and projects.
    /// The request is scoped to the current tenant context.
    ///
    /// Sample request:
    ///
    ///     GET /api/teams/team/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="teamId">The unique identifier of the team.</param>
    /// <returns>The full information of the requested team.</returns>
    /// <response code="200">Returns the team information successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the team with the specified ID does not exist.</response>
    [Authorize(Policy = Policies.Teams_Read)]
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<TeamFullInfoDto>> GetTeam(string teamId)
    {
        var query = new GetTeam.Query
        {
            TeamId = teamId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Adds members to an existing team within the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint allows authorized roles to add one or more users to a team.
    /// The operation is scoped to the current tenant context.
    ///
    /// Sample request:
    ///
    ///     POST /api/teams/team/members
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "teamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "memberIds": [
    ///         "11111111-1111-1111-1111-111111111111",
    ///         "22222222-2222-2222-2222-222222222222"
    ///       ]
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The team id and the member ids to add.</param>
    /// <returns>A success message if members were added.</returns>
    /// <response code="200">Members were added successfully.</response>
    /// <response code="400">If validation fails (duplicates, capacity exceeded, inactive team, etc.).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If the team or any user is not found in the tenant.</response>
    [Authorize(Policy = Policies.Teams_Manage)]
    [HttpPost("team/members")]
    public async Task<ActionResult<string>> AddMembersToTeam(AddMembersToTeamRequest request)
    {
        var command = Mapper.Map<AddMembersToTeam.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Removes members from an existing team within the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint allows authorized roles to remove one or more users from a team.
    /// The operation is scoped to the current tenant context.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/teams/team/members
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "teamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "membersIds": [
    ///         "11111111-1111-1111-1111-111111111111",
    ///         "22222222-2222-2222-2222-222222222222"
    ///       ]
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The team id and the member ids to remove.</param>
    /// <returns>A success message if members were removed.</returns>
    /// <response code="200">Members were removed successfully.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If the team or any user is not found in the tenant.</response>
    [Authorize(Policy = Policies.Teams_Manage)]
    [HttpPatch("team/members")]
    public async Task<ActionResult<string>> RemoveMembersFromTeam(RemoveMembersFromTeamRequest request)
    {
        var command = Mapper.Map<RemoveMembersFromTeam.Command>(request);

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves teams for the current tenant with filtering and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// This endpoint returns teams scoped to the current tenant context, with optional filters.
    /// 
    /// Sample request:
    ///
    ///     GET /api/teams?nameContains=Unity&amp;department=SoftwareDevelopment&amp;isActive=true&amp;pageSize=10
    ///
    /// Cursor pagination:
    ///
    ///     GET /api/teams?cursor=2026-01-01T00:00:00Z&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="request">Filter and pagination parameters.</param>
    /// <returns>A paged list of teams.</returns>
    /// <response code="200">Returns teams successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the tenant does not exist.</response>
    [Authorize(Policy = Policies.Teams_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<TeamDto, DateTime?>>> GetTeams([FromQuery] GetTeamsRequest request)
    {
        var query = Mapper.Map<GetTeams.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Updates an existing team within the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint updates team details such as name, description, department, capacity, and team lead.
    /// The update is scoped to the current tenant context.
    ///
    /// Sample request:
    ///
    ///     PUT /api/teams/team
    ///     Content-RelationType: application/json
    ///
    ///     {
    ///       "teamLeadId": "11111111-1111-1111-1111-111111111111",
    ///       "name": "Unity Team - Updated",
    ///       "description": "Updated team description",
    ///       "department": "SoftwareDevelopment",
    ///       "capacity": 10
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The Team to update.</param>
    /// <param name="request">The update payload.</param>
    /// <returns>No content if updated successfully.</returns>
    /// <response code="204">Team updated successfully.</response>
    /// <response code="400">If validation fails (duplicate name, invalid data, etc.).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If tenant, team, or team lead is not found.</response>
    [Authorize(Policy = Policies.Teams_Manage)]
    [HttpPatch("team/{id}")]
    public async Task<ActionResult> UpdateTeam(string id, UpdateTeamRequest request)
    {
        var command = Mapper.Map<UpdateTeam.Command>(request);
        command.Id = id;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Toggles the active status of a team.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to Product Team Manager Or Tenant Admin for administrative control.
    /// It allows enabling or disabling any team in the system by its identifier.
    ///
    /// Sample request:
    ///
    ///     PUT /api/teams/3fa85f64-5717-4562-b3fc-2c963f66afa6/toggle-active
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the team whose active status will be toggled.</param>
    /// <returns>No content if the operation succeeds.</returns>
    /// <response code="204">The team active status was toggled successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized.</response>
    /// <response code="404">If the team with the specified ID does not exist.</response>
    [Authorize(Policy = Policies.Teams_Manage)]
    [HttpPatch("{id}/toggle-active")]
    public async Task<ActionResult> ToggleTenantActiveStatus(string id)
    {
        var command = new ToggleTeamActivation.Command
        {
            TeamId = id
        };

        return HandleResult(await Mediator.Send(command));
    }
}