using API.Contracts.Identity;
using API.Contracts.Tenants;
using API.Contracts.Users;
using Application.Features.Identity.Commands;
using Application.Features.Users.Commands;
using AutoMapper;

namespace API.MappingProfiles;

/// <summary>
/// AutoMapper profile for mapping between API contract models and application command models related to identity.
/// </summary>
public class IdentityApiMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityApiMappingProfile"/> class.
    /// </summary>
    public IdentityApiMappingProfile()
    {
        CreateMap<RegisterSuperAdminRequest, RegisterAppUser.Command>();
        CreateMap<RegisterAppUserRequest, RegisterAppUser.Command>();
        CreateMap<RegisterEmployeeRequest, RegisterAppUser.Command>();
        CreateMap<SendPasswordResetEmailRequest, SendPasswordResetEmail.Command>();
        CreateMap<ReSendConfirmationEmailRequest, ReSendConfirmationEmail.Command>();
        CreateMap<ResetPasswordRequest, ResetPassword.Command>();
        CreateMap<ChangePasswordRequest, ChangePassword.Command>();
        CreateMap<ConfirmEmailRequest, ConfirmEmail.Command>();
        CreateMap<LoginRequest, LoginUser.Command>();
        CreateMap<UpdateUserProfileRequest, UpdateUserContactDetails.Command>();
        CreateMap<UploadFileRequest, UploadUserProfileImage.Command>();
    }
}