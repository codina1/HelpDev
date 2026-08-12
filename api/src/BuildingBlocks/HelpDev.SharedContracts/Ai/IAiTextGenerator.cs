namespace HelpDev.SharedContracts.Ai;

/// <summary>
/// Provider-agnostic text generation port. Implementations live in Infrastructure —
/// Content Domain/Application must not reference SDKs or provider types.
/// Prefer <see cref="GenerateSafeAsync"/> for production call sites.
/// </summary>
public interface IAiTextGenerator
{
    Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default);

    /// <summary>Never throws for provider failures — returns <see cref="AiGenerationResult"/>.</summary>
    Task<AiGenerationResult> GenerateSafeAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AiTextRequest(
    string TaskType,
    string SystemInstruction,
    string InputText,
    int MaxTokens);

public sealed record AiTextResponse(
    string Text,
    string Model,
    string Provider,
    AiTokenUsage? Usage);

public sealed record AiTokenUsage(int InputTokens, int OutputTokens);
