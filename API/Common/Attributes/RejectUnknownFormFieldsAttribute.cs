using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace API.Common.Attributes;

/// <summary>
/// For multipart/form-data:
/// 1) Rejects unknown form fields (text + files) not present on the DTO.
/// 2) Prevents repeating the same file field name more than once (e.g. File: img1, File: img2).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RejectUnknownFormFieldsAttribute : ActionFilterAttribute
{
    private readonly HashSet<string> _allowedTopLevelKeys;
    private readonly bool _allowEmptyRequest;
    private readonly bool _disallowDuplicateFileFieldNames;

    public RejectUnknownFormFieldsAttribute(
        Type dtoType,
        bool allowEmptyRequest = true,
        bool disallowDuplicateFileFieldNames = true,
        params string[] extraAllowedKeys)
    {
        if (dtoType is null) throw new ArgumentNullException(nameof(dtoType));

        _allowEmptyRequest = allowEmptyRequest;
        _disallowDuplicateFileFieldNames = disallowDuplicateFileFieldNames;

        _allowedTopLevelKeys = dtoType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var k in extraAllowedKeys ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(k))
                _allowedTopLevelKeys.Add(k.Trim());
        }
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var req = context.HttpContext.Request;

        if (!req.HasFormContentType)
            return;

        var form = req.Form;

        if (!_allowEmptyRequest && form.Keys.Count == 0 && form.Files.Count == 0)
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = "At least one form field must be provided."
            });
            return;
        }

        static string TopLevel(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return key;
            var split = key.Split('.', '[', ']');
            return split.Length > 0 ? split[0] : key;
        }

        var unknownKeys = form.Keys
            .Select(k => new { Original = k, Top = TopLevel(k) })
            .Where(x => !_allowedTopLevelKeys.Contains(x.Top))
            .Select(x => x.Original)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fileNames = form.Files
            .Select(f => f.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        var unknownFileKeys = fileNames
            .Where(name => !_allowedTopLevelKeys.Contains(TopLevel(name)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        List<string> duplicateFileFieldNames = new();
        if (_disallowDuplicateFileFieldNames)
        {
            duplicateFileFieldNames = fileNames
                .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        var allUnknown = unknownKeys
            .Concat(unknownFileKeys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (duplicateFileFieldNames.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Multiple files are not supported. Only one file is allowed."
            });
            return;
        }

        if (allUnknown.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = "Invalid form-data payload.",
                unknownFields = allUnknown
            });
            return;
        }
    }
}