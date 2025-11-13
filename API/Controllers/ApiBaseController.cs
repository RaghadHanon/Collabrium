using Application.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers;

/// <summary>
/// Base API controller providing common functionality for derived controllers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ApiBaseController : ControllerBase
{
    private IMediator? _mediator;
    private IMapper? _mapper;

    /// <summary>
    /// Gets the IMediator instance from the request services.
    /// </summary>
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>()
       ?? throw new InvalidOperationException("IMediator Service is not available!");

    /// <summary>
    /// Gets the IMapper instance from the request services.
    /// </summary>
    protected IMapper Mapper =>
        _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>()
        ?? throw new InvalidOperationException("IMapper Service is not available!");

    /// <summary>
    /// Handles the result from application layer and returns appropriate HTTP response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result">The result object from MediatR or service layer.</param>
    /// <param name="createdAtActionName">Optional: Action name for CreatedAt response.</param>
    /// <param name="createdAtControllerName">Optional: Controller name for CreatedAt response.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected ActionResult HandleResult<T>(Result<T> result, string? createdAtActionName = null, string? createdAtControllerName = null)
    {
        if (result == null)
            throw new InvalidOperationException("A null result was returned from the application.");

        if (result.IsSuccess)
        {
            // SPECIAL CASE: return PDF/File directly
            if (result.Value is FileContentResult fileResult)
            {
                return fileResult;
            }

            switch (result.StatusCode)
            {
                case HttpStatusCode.Created:
                    if (string.IsNullOrEmpty(createdAtActionName))
                        throw new InvalidOperationException(
                            "The 'createdAtActionName' parameter must be provided for a 'Created' result.");

                    var location = Url.Action(
                        action: createdAtActionName,
                        controller: createdAtControllerName ?? ControllerContext.ActionDescriptor.ControllerName,
                        values: new { id = result.Value?.GetType().GetProperty("Id")?.GetValue(result.Value) },
                        protocol: Request.Scheme);

                    return Created(location!, result.Value);

                case HttpStatusCode.NoContent:
                    return NoContent();

                default:
                    return Ok(result.Value);
            }
        }

        var problem = new ProblemDetails
        {
            Title = result.StatusCode.ToString(),
            Detail = result.Error,
            Status = (int) result.StatusCode
        };

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => NotFound(problem),
            HttpStatusCode.BadRequest => BadRequest(problem),
            HttpStatusCode.Unauthorized => Unauthorized(problem),
            HttpStatusCode.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            HttpStatusCode.Conflict => Conflict(problem),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = result.Error ?? "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError
                })
        };
    }
}