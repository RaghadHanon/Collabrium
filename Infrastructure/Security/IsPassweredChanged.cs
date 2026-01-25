using Application.Common.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

public class IsPasswordChangedRequirement : IAuthorizationRequirement
{ }

public class IsPasswordChangedHandler
    : AuthorizationHandler<IsPasswordChangedRequirement>
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly UserManager<ApplicationUserEntity> _userManager;

    public IsPasswordChangedHandler(
        ICurrentUserAccessor currentUser,
        UserManager<ApplicationUserEntity> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    IsPasswordChangedRequirement requirement)
    {
        var userId = _currentUser.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        if (context.User.IsInRole(nameof(UserRole.SuperAdmin)))
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

        if (user.MustChangePassword)
        {
            throw new MustChangePasswordException();
        }

        context.Succeed(requirement);
    }
}
