using HelpDev.Modules.Search.Application.Rag;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Search.Tests.Rag;

public sealed class RagAnswerServiceTests
{
    [Fact]
    public async Task Ask_uses_only_retrieved_context_and_audits_source_count()
    {
        var sourceId = Guid.NewGuid();
        var contextBuilder = new StubContextBuilder
        {
            Context = new RagContext(
                "q",
                [
                    new RagContextSource(
                        "Doc",
                        "content",
                        sourceId,
                        "/content/pg",
                        "HelpDev uses PostgreSQL.",
                        0.9),
                ],
                6000),
        };
        var ai = new StubAi();
        var audit = new StubAudit();
        var usage = new StubUsage();
        var service = new RagAnswerService(
            contextBuilder,
            ai,
            usage,
            audit,
            new FixedClock(new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc)),
            NullLogger<RagAnswerService>.Instance);

        var result = await service.AskAsync("How does HelpDev store data?");

        Assert.Equal("grounded-answer", result.Answer);
        Assert.Single(result.Sources);
        Assert.Contains("HelpDev uses PostgreSQL", ai.LastRequest!.InputText, StringComparison.Ordinal);
        Assert.Contains("ONLY the provided HelpDev knowledge", ai.LastRequest.SystemInstruction, StringComparison.Ordinal);
        Assert.Equal(AuditActions.RagAnswerRequested, Assert.Single(audit.Records).Action);
        Assert.Equal("1", audit.Records[0].Metadata!["sourceCount"]);
        Assert.False(audit.Records[0].Metadata!.ContainsKey("prompt"));
        Assert.False(audit.Records[0].Metadata!.ContainsKey("question"));
    }

    [Fact]
    public async Task Ask_without_hits_returns_safe_empty_answer()
    {
        var audit = new StubAudit();
        var service = new RagAnswerService(
            new StubContextBuilder { Context = new RagContext("q", [], 6000) },
            new StubAi(),
            new StubUsage(),
            audit,
            new FixedClock(DateTime.UtcNow),
            NullLogger<RagAnswerService>.Instance);

        var result = await service.AskAsync("unknown topic?");

        Assert.Contains("پیدا نشد", result.Answer, StringComparison.Ordinal);
        Assert.Empty(result.Sources);
        Assert.Equal("0", Assert.Single(audit.Records).Metadata!["sourceCount"]);
    }

    private sealed class FixedClock(DateTime utc) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utc;
    }

    private sealed class StubContextBuilder : IRagContextBuilder
    {
        public RagContext Context { get; set; } = new("q", [], 6000);

        public Task<RagContext> BuildAsync(string question, CancellationToken cancellationToken = default) =>
            Task.FromResult(Context);
    }

    private sealed class StubAi : IAiTextGenerator
    {
        public AiTextRequest? LastRequest { get; private set; }

        public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new AiTextResponse("grounded-answer", "fake-v1", "Fake", null));
        }

        public Task<AiGenerationResult> GenerateSafeAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(AiGenerationResult.Ok("grounded-answer", 5, "fake-v1", "Fake", null));
        }
    }

    private sealed class StubUsage : IAiUsageRecorder
    {
        public List<AiUsageRecordInput> Records { get; } = [];

        public Task RecordAsync(AiUsageRecordInput input, CancellationToken cancellationToken = default)
        {
            Records.Add(input);
            return Task.CompletedTask;
        }
    }

    private sealed class StubAudit : IAuditRecorder
    {
        public List<AuditRecordInput> Records { get; } = [];

        public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default)
        {
            Records.Add(input);
            return Task.CompletedTask;
        }
    }
}
