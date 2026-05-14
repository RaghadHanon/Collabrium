namespace Application.Common.DTOs;

public record StoredFileResult(
    string StoragePath,
    string Url,
    string ContentType,
    long Size,
    string FileName
);