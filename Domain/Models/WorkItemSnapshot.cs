using Domain.Enums;

namespace Domain.Models;

public sealed record WorkItemSnapshot(
    string Id,
    WorkItemType Type,
    WorkItemStatus Status,
    TestCaseResult? TestCaseResult = null
);