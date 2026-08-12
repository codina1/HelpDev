using HelpDev.Modules.Content.Domain.AiWorkflow;

namespace HelpDev.Modules.Content.Application.AiWorkflow;

public sealed record ContentIdeaDto(
    Guid Id,
    string Title,
    string Description,
    string TargetType,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record AiContentWorkflowSessionDto(
    Guid Id,
    Guid IdeaId,
    string CurrentStep,
    Guid CreatedByUserId,
    Guid? LinkedContentId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    ContentIdeaDto Idea);

public sealed record AiContentWorkflowListItemDto(
    Guid Id,
    Guid IdeaId,
    string IdeaTitle,
    string IdeaStatus,
    string CurrentStep,
    Guid CreatedByUserId,
    Guid? LinkedContentId,
    DateTime UpdatedAtUtc);

public sealed record CreateAiContentWorkflowRequest(
    string Title,
    string? Description,
    string? TargetType);

public sealed record AiResearchSourceDto(
    string Title,
    string Url,
    string SourceType,
    string Snippet);

public sealed record AiResearchResultDto(
    string Summary,
    IReadOnlyList<AiResearchSourceDto> Sources,
    string Model,
    string Provider,
    DateTime CreatedAtUtc);

public sealed record ContentOutlineSectionDto(
    string Heading,
    IReadOnlyList<string> Subheadings);

public sealed record ContentOutlineDto(
    string Title,
    IReadOnlyList<ContentOutlineSectionDto> Sections,
    string RawText,
    string Model,
    string Provider,
    DateTime CreatedAtUtc);

public sealed record GenerateOutlineRequest(string? ResearchSummary);

public sealed record DraftSuggestionDto(
    string Title,
    string BodyMarkdown,
    string Model,
    string Provider,
    DateTime CreatedAtUtc);

public sealed record GenerateDraftRequest(
    string OutlineTitle,
    string OutlineText);

public sealed record SeoOptimizationSuggestionDto(
    string? SuggestedTitle,
    string? SuggestedDescription,
    IReadOnlyList<string> KeywordSuggestions,
    IReadOnlyList<string> Recommendations,
    DateTime CreatedAtUtc);

public sealed record GenerateSeoRequest(
    string Title,
    string Body,
    string? Slug,
    string? FocusKeyword);

public sealed record ApplyDraftRequest(
    string Title,
    string Body,
    string? Slug,
    string? TargetType);

public sealed record ApplyDraftResultDto(
    Guid WorkflowId,
    Guid ContentId,
    int RevisionVersion,
    string ContentStatus);

public static class AiWorkflowTaskTypes
{
    public const string Research = "WorkflowResearch";
    public const string Outline = "WorkflowOutline";
    public const string Draft = "WorkflowDraft";
    public const string Seo = "WorkflowSeo";
}

public static class AiWorkflowMapper
{
    public static ContentIdeaDto ToDto(ContentIdea idea) =>
        new(
            idea.Id,
            idea.Title,
            idea.Description,
            idea.TargetType,
            idea.Status.ToString(),
            idea.CreatedByUserId,
            idea.CreatedAtUtc,
            idea.UpdatedAtUtc);

    public static AiContentWorkflowSessionDto ToDto(AiContentWorkflowSession session, ContentIdea idea) =>
        new(
            session.Id,
            session.IdeaId,
            session.CurrentStep.ToString(),
            session.CreatedByUserId,
            session.LinkedContentId,
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            ToDto(idea));

    public static AiContentWorkflowListItemDto ToListItem(AiContentWorkflowSession session, ContentIdea idea) =>
        new(
            session.Id,
            session.IdeaId,
            idea.Title,
            idea.Status.ToString(),
            session.CurrentStep.ToString(),
            session.CreatedByUserId,
            session.LinkedContentId,
            session.UpdatedAtUtc);
}
