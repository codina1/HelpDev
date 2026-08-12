namespace HelpDev.API.Contracts;

/// <summary>
/// Canonical API error response returned by HelpDev endpoints.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>
    /// Human-readable message suitable for client display.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Stable machine-readable error code.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}
