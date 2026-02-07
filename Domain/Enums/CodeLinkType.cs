namespace Domain.Enums;

public enum CodeLinkType
{
    Repository = 0,
    Branch = 1,
    Commit = 2,
    PullRequest = 3,
    Pipeline = 4,
    Other = 99
}