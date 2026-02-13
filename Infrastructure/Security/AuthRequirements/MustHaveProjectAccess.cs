using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class MustHaveProjectAccessRequirement : IAuthorizationRequirement
{ }

public class MustHaveProjectAccessHandler
    : AuthorizationHandler<MustHaveProjectAccessRequirement>
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly IProjectsRepository _projectsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MustHaveProjectAccessHandler(
        ICurrentUserAccessor currentUser,
        UserManager<ApplicationUserEntity> userManager,
        IProjectsRepository projectsRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _projectsRepository = projectsRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    MustHaveProjectAccessRequirement requirement)
    {
        var userId = _currentUser.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        if (context.User.IsInRole(nameof(UserRole.SuperAdmin)))
        {
            context.Fail();
            return;
        }

        if (context.User.IsInRole(nameof(UserRole.TenantAdmin))
            || context.User.IsInRole(nameof(UserRole.CEO)))
        {
            context.Succeed(requirement);
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            context.Fail();
            return;
        }

        var projectIdString = _httpContextAccessor.HttpContext?.Request.RouteValues["projectId"]?.ToString();
        if (string.IsNullOrEmpty(projectIdString) || !Guid.TryParse(projectIdString, out var projectId))
        {
            context.Fail();
            return;
        }

        if(!(await _projectsRepository.IsProjectExists(user!.TenantId!, projectIdString)))
        {
            throw new NotFoundException("Project not found.");
        }

        var hasAccess = await _projectsRepository.IsUserHasAccessToProjectAsync(user!.TenantId!, userId, projectIdString);

        if (!hasAccess)
        {
            throw new NoAccessToTheProjectException();
        }

        context.Succeed(requirement);
    }
}