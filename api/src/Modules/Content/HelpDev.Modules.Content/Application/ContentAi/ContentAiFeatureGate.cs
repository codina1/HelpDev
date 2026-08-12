using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;

namespace HelpDev.Modules.Content.Application.ContentAi;

/// <summary>Runtime feature gate for Content AI (no secrets, no provider types).</summary>
public interface IContentAiFeatureGate
{
    bool IsEnabled { get; }

    string DefaultModel { get; }

    bool IsTaskAllowed(ContentAiTaskType taskType);
}

public sealed class ContentAiException : Exception
{
    public ContentAiException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class ContentAiErrorCodes
{
    public const string Disabled = "content_ai_disabled";
    public const string TaskNotAllowed = "content_ai_task_not_allowed";
    public const string ProviderFailed = "content_ai_provider_failed";
    public const string NotFound = "content_not_found";
}
