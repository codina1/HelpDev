namespace HelpDev.SharedContracts.Ai;

/// <summary>
/// Provider-agnostic embedding port. Implementations live in Infrastructure.
/// Search Application depends on this contract only — never on provider SDKs.
/// </summary>
public interface IEmbeddingGenerator
{
    Task<EmbeddingResult> GenerateAsync(string text, CancellationToken cancellationToken = default);
}

public sealed record EmbeddingResult(
    float[] Vector,
    int Dimensions,
    string Model,
    string Provider);
