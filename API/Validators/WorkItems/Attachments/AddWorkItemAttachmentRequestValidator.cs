using API.Contracts.WorkItems.Attachments;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Attachments;

public class AddWorkItemAttachmentRequestValidator
    : AbstractValidator<AddWorkItemAttachmentRequest>
{
    public AddWorkItemAttachmentRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must( v => Enum.TryParse<AttachmentType>(v, true, out _))
            .WithMessage("Invalid attachment type.");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.")
            .Must(BeAllowedFile)
            .WithMessage("File type does not match the specified attachment type.");
    }

    private static bool BeAllowedFile(AddWorkItemAttachmentRequest request, IFormFile file)
    {
        if (!Enum.TryParse<AttachmentType>(request.Type, true, out var attachmentType))
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return attachmentType switch
        {
            AttachmentType.Image =>
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                    .Contains(extension),

            AttachmentType.Video =>
                new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" }
                    .Contains(extension),

            AttachmentType.Document =>
                new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" }
                    .Contains(extension),

            AttachmentType.Log =>
                new[] { ".log", ".txt" }
                    .Contains(extension),

            _ => false
        };
    }
}