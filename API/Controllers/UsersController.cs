using API.SetupProgram;
using Application.Features.Identity.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
public class UsersController : ApiBaseController
{
    /// <summary>
    /// Retrieves the profile of the currently logged-in user.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/auth/users/me
    /// 
    /// </remarks>
    /// <returns>The details of the current user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user has not changed their initial password.</response>
    [Authorize(Policy = Policies.PasswordChanged)]
    [HttpGet("users/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetUser.Query();

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves the details of a specific user by their ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/auth/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>The details of the specified user.</returns>
    /// <response code="200">Returns the user details successfully.</response>
    /// <response code="404">If the user with the specified ID was not found.</response>
    /// <response code="403">If the requesting user has not changed their initial password.</response>
    [Authorize(Policy = Policies.PasswordChanged)]
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var query = new GetUser.Query();
        query.UserId = id;

        return HandleResult(await Mediator.Send(query));
    }
}
