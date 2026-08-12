using System.Diagnostics;
using HelpDev.SharedContracts.Ai;

namespace HelpDev.Infrastructure.Ai;

/// <summary>Maps throwing generators into <see cref="AiGenerationResult"/> without logging content.</summary>
public static class AiTextGeneratorCompat
{
    public static async Task<AiGenerationResult> SafeFromAsync(
        Func<CancellationToken, Task<AiTextResponse>> generate,
        string provider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(generate);
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await generate(cancellationToken);
            sw.Stop();
            if (string.IsNullOrWhiteSpace(response.Text))
            {
                return AiGenerationResult.Fail(
                    AiErrorCodes.InvalidResponse,
                    sw.ElapsedMilliseconds,
                    provider,
                    response.Model);
            }

            return AiGenerationResult.Ok(
                response.Text,
                sw.ElapsedMilliseconds,
                response.Model,
                response.Provider,
                response.Usage);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return AiGenerationResult.Fail(AiErrorCodes.Timeout, sw.ElapsedMilliseconds, provider);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            return AiGenerationResult.Fail(
                MapErrorCode(ex),
                sw.ElapsedMilliseconds,
                provider);
        }
    }

    public static string MapErrorCode(Exception ex)
    {
        var message = (ex.Message ?? string.Empty).Trim();
        if (message is AiErrorCodes.ProviderUnavailable
            or AiErrorCodes.GenerationFailed
            or AiErrorCodes.Timeout
            or AiErrorCodes.InvalidResponse
            or AiErrorCodes.Disabled
            or AiErrorCodes.Unauthorized
            or AiErrorCodes.ValidationFailed)
        {
            return message;
        }

        if (message.Contains("disabled", StringComparison.OrdinalIgnoreCase))
        {
            return AiErrorCodes.Disabled;
        }

        if (message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
            || message.Contains("401", StringComparison.OrdinalIgnoreCase)
            || message.Contains("403", StringComparison.OrdinalIgnoreCase))
        {
            return AiErrorCodes.Unauthorized;
        }

        if (message.Contains("empty response", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
        {
            return AiErrorCodes.InvalidResponse;
        }

        if (message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || ex is TimeoutException)
        {
            return AiErrorCodes.Timeout;
        }

        if (message.Contains("not configured", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unavailable", StringComparison.OrdinalIgnoreCase)
            || ex is HttpRequestException)
        {
            return AiErrorCodes.ProviderUnavailable;
        }

        return AiErrorCodes.GenerationFailed;
    }
}
