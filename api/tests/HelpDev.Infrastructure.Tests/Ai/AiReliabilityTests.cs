using HelpDev.Infrastructure.Ai;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Tests.Ai;

public sealed class AiRetryPolicyTests
{
    [Fact]
    public async Task Retries_transient_failures_then_succeeds()
    {
        var attempts = 0;
        var policy = new AiRetryPolicy(maxAttempts: 3, baseDelayMs: 1, maxDelayMs: 2);

        var result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3)
            {
                return Task.FromResult(AiGenerationResult.Fail(AiErrorCodes.Timeout, 5, "Fake"));
            }

            return Task.FromResult(AiGenerationResult.Ok("ok", 5, "m", "Fake", null));
        });

        Assert.True(result.Success);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task Does_not_retry_unauthorized()
    {
        var attempts = 0;
        var policy = new AiRetryPolicy(maxAttempts: 3, baseDelayMs: 1, maxDelayMs: 2);

        var result = await policy.ExecuteAsync(_ =>
        {
            attempts++;
            return Task.FromResult(AiGenerationResult.Fail(AiErrorCodes.Unauthorized, 1, "Http"));
        });

        Assert.False(result.Success);
        Assert.Equal(AiErrorCodes.Unauthorized, result.ErrorCode);
        Assert.Equal(1, attempts);
    }
}

public sealed class AiHealthProbeTests
{
    [Fact]
    public async Task Fake_provider_is_healthy_without_generation()
    {
        var options = Options.Create(new AiProviderOptions
        {
            Enabled = true,
            ProviderName = "Fake",
            Model = "fake-v1",
        });
        var metrics = new AiOperationMetrics(options, new FixedClock());
        var probe = new AiHealthProbe(
            options,
            new StubHttpClientFactory(),
            metrics,
            new FixedClock());

        var result = await probe.CheckAsync();

        Assert.Equal(OperationalHealthStates.Healthy, result.Status);
        Assert.Null(result.Code);
    }

    [Fact]
    public async Task Disabled_provider_is_degraded()
    {
        var options = Options.Create(new AiProviderOptions
        {
            Enabled = false,
            ProviderName = "Fake",
            Model = "fake-v1",
        });
        var probe = new AiHealthProbe(
            options,
            new StubHttpClientFactory(),
            new AiOperationMetrics(options, new FixedClock()),
            new FixedClock());

        var result = await probe.CheckAsync();

        Assert.Equal(OperationalHealthStates.Degraded, result.Status);
        Assert.Equal(HealthCheckCodes.AiDisabled, result.Code);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}

public sealed class ResilientAiTextGeneratorTests
{
    [Fact]
    public async Task GenerateSafe_records_metrics_and_never_throws_on_provider_failure()
    {
        var options = Options.Create(new AiProviderOptions
        {
            Enabled = true,
            ProviderName = "Fake",
            Model = "fake-v1",
        });
        var metrics = new AiOperationMetrics(options, new FixedClock());
        var inner = new FailingInner();
        var resilient = new ResilientAiTextGenerator(
            inner,
            new AiRetryPolicy(maxAttempts: 2, baseDelayMs: 1, maxDelayMs: 2),
            metrics,
            NullLogger<ResilientAiTextGenerator>.Instance);

        var result = await resilient.GenerateSafeAsync(
            new AiTextRequest("ContentAssistant", "sys", "in", 64));

        Assert.False(result.Success);
        Assert.Equal(AiErrorCodes.Timeout, result.ErrorCode);
        var snap = metrics.GetSnapshot();
        Assert.Equal(1, snap.TotalRequests);
        Assert.Equal(1, snap.FailureCount);
        Assert.DoesNotContain("sys", snap.ProviderName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fake_failure_injector_surfaces_stable_error_code()
    {
        var injector = new FakeAiFailureInjector();
        injector.Arm(AiErrorCodes.ProviderUnavailable, 1);
        var generator = new FakeAiTextGenerator(
            Options.Create(new AiProviderOptions
            {
                Enabled = true,
                ProviderName = "Fake",
                Model = "fake-v1",
            }),
            injector);

        var result = await generator.GenerateSafeAsync(
            new AiTextRequest("WorkflowDraft", "sys", "Title: demo", 64));

        Assert.False(result.Success);
        Assert.Equal(AiErrorCodes.ProviderUnavailable, result.ErrorCode);
        Assert.Null(result.Content);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FailingInner : IAiTextGenerator
    {
        public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            throw new TimeoutException("timeout");

        public Task<AiGenerationResult> GenerateSafeAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AiGenerationResult.Fail(AiErrorCodes.Timeout, 3, "Fake"));
    }
}

public sealed class AiPolicyTests
{
    [Fact]
    public void Policy_rules_include_human_approval_and_no_auto_publish()
    {
        Assert.Contains(AiPolicy.Rules, r => r.Contains("approval", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(AiPolicy.Rules, r => r.Contains("auto-publish", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(AiPolicy.Rules, r => r.Contains("suggestion", StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class AiSecurityLoggingTests
{
    [Fact]
    public void Error_codes_do_not_resemble_secrets()
    {
        Assert.DoesNotContain("sk-", AiErrorCodes.ProviderUnavailable, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer", AiErrorCodes.Unauthorized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("prompt", AiErrorCodes.InvalidResponse, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Usage_input_contract_has_no_prompt_or_content_fields()
    {
        var names = typeof(AiUsageRecordInput).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        Assert.DoesNotContain("Prompt", names);
        Assert.DoesNotContain("GeneratedText", names);
        Assert.DoesNotContain("ApiKey", names);
        Assert.DoesNotContain("Answer", names);
        Assert.Contains("Success", names);
        Assert.Contains("DurationMs", names);
        Assert.Contains("ErrorCode", names);
    }
}
