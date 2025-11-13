using Application.Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text;

namespace Application.Features.Identity.Commands;

public partial class ConfirmEmail
{
    public class Command : IRequest<Result<string>>
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<string>.Failure("User not found.", HttpStatusCode.BadRequest);

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Result<string>.Failure($"Email confirmation failed: {errors}", HttpStatusCode.BadRequest);
            }

            return Result<string>.Success("Email confirmed successfully!", HttpStatusCode.OK);
        }
    }
}
