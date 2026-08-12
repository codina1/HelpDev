namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed record ContentWorkflowTransitionDto(
    Guid Id,
    string FromStatus,
    string ToStatus,
    Guid ActorUserId,
    string? Comment,
    DateTime CreatedAtUtc);

public sealed record WorkflowHistoryDto(
    IReadOnlyList<ContentWorkflowTransitionDto> Items);

public sealed record RejectContentRequest(string Comment);
