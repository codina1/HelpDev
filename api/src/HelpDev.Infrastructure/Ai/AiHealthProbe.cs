using System.Diagnostics;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// AI health probe — configuration validation and lightweight connectivity only.
/// Never issues a generation request.
/// </summary>
public sealed class AiHealthProbe : IAiHealthProbe
{
    private readonly IOptions<AiProviderOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAiOperationMetrics _metrics;
    private readonly IDateTimeProvider _clock;

    public AiHealthProbe(
        IOptions<AiProviderOptions> options,
        IHttpClientFactory httpClientFactory,
        IAiOperationMetrics metrics,
        IDateTimeProvider clock)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _metrics = metrics;
        _clock = clock;
    }

    public async Task<AiHealthProbeResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var options = _options.Value;
        var provider = (options.ProviderName ?? "Fake").Trim();
        var snapshot = _metrics.GetSnapshot();
        var details = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["provider"] = provider,
            ["enabled"] = options.Enabled ? "true" : "false",
            ["modelConfigured"] = string.IsNullOrWhiteSpace(options.Model) ? "false" : "true",
            ["totalRequests"] = snapshot.TotalRequests.ToString(),
            ["successRate"] = snapshot.SuccessRate.ToString("0.####"),
        };

        if (!options.Enabled)
        {
            sw.Stop();
            return new AiHealthProbeResult(
                OperationalHealthStates.Degraded,
                HealthCheckCodes.AiDisabled,
                "AI provider is disabled.",
                sw.ElapsedMilliseconds,
                _clock.UtcNow,
                details);
        }

        if (string.Equals(provider, "Fake", StringComparison.OrdinalIgnoreCase))
        {
            sw.Stop();
            return new AiHealthProbeResult(
                OperationalHealthStates.Healthy,
                null,
                "Fake AI provider is configured.",
                sw.ElapsedMilliseconds,
                _clock.UtcNow,
                details);
        }

        if (string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.Endpoint)
                || !Uri.TryCreate(options.Endpoint.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                sw.Stop();
                return new AiHealthProbeResult(
                    OperationalHealthStates.Unhealthy,
                    HealthCheckCodes.ConfigurationInvalid,
                    "AI HTTP endpoint is not configured.",
                    sw.ElapsedMilliseconds,
                    _clock.UtcNow,
                    details);
            }

            // Connectivity only: OPTIONS/HEAD-style probe via GET with short timeout.
            // Never POST generation payloads.
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                var client = _httpClientFactory.CreateClient(nameof(HttpAiTextGenerator));
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cts.Token);

                details["httpStatus"] = ((int)response.StatusCode).ToString();
                sw.Stop();

                // 405 Method Not Allowed still proves the host is reachable.
                if ((int)response.StatusCode >= 500)
                {
                    return new AiHealthProbeResult(
                        OperationalHealthStates.Unhealthy,
                        HealthCheckCodes.AiUnavailable,
                        "AI HTTP provider returned a server error.",
                        sw.ElapsedMilliseconds,
                        _clock.UtcNow,
                        details);
                }

                return new AiHealthProbeResult(
                    OperationalHealthStates.Healthy,
                    null,
                    "AI HTTP provider endpoint is reachable.",
                    sw.ElapsedMilliseconds,
                    _clock.UtcNow,
                    details);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                sw.Stop();
                return new AiHealthProbeResult(
                    OperationalHealthStates.Degraded,
                    HealthCheckCodes.Timeout,
                    "AI HTTP connectivity check timed out.",
                    sw.ElapsedMilliseconds,
                    _clock.UtcNow,
                    details);
            }
            catch (Exception)
            {
                sw.Stop();
                return new AiHealthProbeResult(
                    OperationalHealthStates.Unhealthy,
                    HealthCheckCodes.AiUnavailable,
                    "AI HTTP provider is unreachable.",
                    sw.ElapsedMilliseconds,
                    _clock.UtcNow,
                    details);
            }
        }

        sw.Stop();
        return new AiHealthProbeResult(
            OperationalHealthStates.Unhealthy,
            HealthCheckCodes.ConfigurationInvalid,
            "Unknown AI provider.",
            sw.ElapsedMilliseconds,
            _clock.UtcNow,
            details);
    }
}
