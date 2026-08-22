using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Content.Tests.ContentAi;

public sealed class ContentAiAssistantServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Analyze_returns_generated_text_and_records_usage_and_audit()
    {
        var contentId = Guid.NewGuid();
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);
        var content = CreateDetail(contentId, actor.UserId);
        var generator = new StubAiTextGenerator(
            new AiTextResponse("analysis text", "fake-v1", "Fake", new AiTokenUsage(10, 20)));
        var usage = new RecordingUsageRecorder();
        var audit = new RecordingAuditRecorder();
        var service = CreateService(content, generator, usage, audit, enabled: true);

        var result = await service.AnalyzeContentAsync(actor, contentId);

        Assert.Equal("ContentAnalysis", result.TaskType);
        Assert.Equal("analysis text", result.GeneratedText);
        Assert.Equal("fake-v1", result.Model);
        Assert.Equal("Fake", result.Provider);
        Assert.Equal(Now, result.CreatedAtUtc);
        Assert.Single(usage.Records);
        Assert.Equal(AiOperationNames.ContentAssistant, usage.Records[0].TaskType);
        Assert.True(usage.Records[0].Success);
        Assert.Contains(audit.Records, r => r.Action == AuditActions.ContentAiTaskRequested);
        Assert.Equal("ContentAnalysis", generator.LastRequest!.TaskType);
    }

    [Fact]
    public async Task Disabled_gate_throws_without_calling_provider()
    {
        var contentId = Guid.NewGuid();
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);
        var generator = new StubAiTextGenerator(new AiTextResponse("x", "m", "Fake", null));
        var service = CreateService(CreateDetail(contentId, actor.UserId), generator, enabled: false);

        var ex = await Assert.ThrowsAsync<ContentAiException>(
            () => service.GenerateOutlineAsync(actor, contentId));

        Assert.Equal(ContentAiErrorCodes.Disabled, ex.Code);
        Assert.Null(generator.LastRequest);
    }

    [Fact]
    public async Task Disallowed_task_throws()
    {
        var contentId = Guid.NewGuid();
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);
        var generator = new StubAiTextGenerator(new AiTextResponse("x", "m", "Fake", null));
        var gate = new StubFeatureGate(enabled: true, allowed: [ContentAiTaskType.ContentAnalysis]);
        var service = CreateService(
            CreateDetail(contentId, actor.UserId),
            generator,
            enabled: true,
            gate: gate);

        var ex = await Assert.ThrowsAsync<ContentAiException>(
            () => service.GenerateFaqAsync(actor, contentId));

        Assert.Equal(ContentAiErrorCodes.TaskNotAllowed, ex.Code);
        Assert.Null(generator.LastRequest);
    }

    [Fact]
    public async Task Provider_failure_audits_failure_and_masks_error()
    {
        var contentId = Guid.NewGuid();
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);
        var generator = new StubAiTextGenerator(failWith: new InvalidOperationException("provider boom with secret"));
        var audit = new RecordingAuditRecorder();
        var service = CreateService(CreateDetail(contentId, actor.UserId), generator, audit: audit);

        var ex = await Assert.ThrowsAsync<ContentAiException>(
            () => service.GenerateTitleSuggestionsAsync(actor, contentId));

        Assert.Equal(ContentAiErrorCodes.ProviderFailed, ex.Code);
        Assert.DoesNotContain("secret", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(audit.Records, r => r.Action == AuditActions.ContentAiTaskFailed);
        var failed = audit.Records.Single(r => r.Action == AuditActions.ContentAiTaskFailed);
        Assert.Equal("TitleSuggestion", failed.Metadata!["taskType"]);
        Assert.Equal(contentId.ToString("D"), failed.Metadata["contentId"]);
        Assert.False(failed.Metadata.ContainsKey("prompt"));
        Assert.False(failed.Metadata.ContainsKey("output"));
        Assert.All(failed.Metadata.Values, v => Assert.DoesNotContain("secret", v, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Ownership_not_found_propagates_from_content_service()
    {
        var contentService = new StubContentService
        {
            ExceptionToThrow = new ContentException("not found", ContentErrorCodes.NotFound),
        };
        var service = new ContentAiAssistantService(
            contentService,
            new StubAiTextGenerator(new AiTextResponse("x", "m", "Fake", null)),
            new StubFeatureGate(true),
            new RecordingUsageRecorder(),
            new RecordingAuditRecorder(),
            new FixedClock(Now),
            NullLogger<ContentAiAssistantService>.Instance);

        var ex = await Assert.ThrowsAsync<ContentException>(
            () => service.AnalyzeContentAsync(
                new ContentManagementActor(Guid.NewGuid(), false),
                Guid.NewGuid()));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Theory]
    [InlineData(ContentAiTaskType.ContentAnalysis, nameof(IContentAiAssistantService.AnalyzeContentAsync))]
    [InlineData(ContentAiTaskType.TitleSuggestion, nameof(IContentAiAssistantService.GenerateTitleSuggestionsAsync))]
    [InlineData(ContentAiTaskType.MetaDescription, nameof(IContentAiAssistantService.GenerateMetaDescriptionAsync))]
    [InlineData(ContentAiTaskType.OutlineGeneration, nameof(IContentAiAssistantService.GenerateOutlineAsync))]
    [InlineData(ContentAiTaskType.FaqGeneration, nameof(IContentAiAssistantService.GenerateFaqAsync))]
    public async Task Task_selection_maps_to_wire_task_type(ContentAiTaskType taskType, string methodName)
    {
        var contentId = Guid.NewGuid();
        var actor = new ContentManagementActor(Guid.NewGuid(), true);
        var generator = new StubAiTextGenerator(new AiTextResponse("ok", "fake-v1", "Fake", null));
        var service = CreateService(CreateDetail(contentId, actor.UserId), generator);
        var method = typeof(IContentAiAssistantService).GetMethod(methodName)!;

        await (Task<ContentAiResultDto>)method.Invoke(service, [actor, contentId, CancellationToken.None])!;

        Assert.Equal(ContentAiTaskTypeCatalog.ToWireName(taskType), generator.LastRequest!.TaskType);
    }

    private static ContentAiAssistantService CreateService(
        AdminContentDetailDto detail,
        StubAiTextGenerator generator,
        RecordingUsageRecorder? usage = null,
        RecordingAuditRecorder? audit = null,
        bool enabled = true,
        StubFeatureGate? gate = null) =>
        new(
            new StubContentService { DetailToReturn = detail },
            generator,
            gate ?? new StubFeatureGate(enabled),
            usage ?? new RecordingUsageRecorder(),
            audit ?? new RecordingAuditRecorder(),
            new FixedClock(Now),
            NullLogger<ContentAiAssistantService>.Instance);

    private static AdminContentDetailDto CreateDetail(Guid id, Guid authorId) =>
        new(
            id,
            "عنوان نمونه",
            "sample-slug",
            "بدنه مقاله برای تست",
            "خلاصه",
            null,
            "Article",
            "Draft",
            authorId,
            0,
            0,
            Now,
            Now,
            null,
            new SeoMetadataDto("seo", "desc", null, null, null));

    private sealed class FixedClock(DateTime utc) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utc;
    }

    private sealed class StubFeatureGate : IContentAiFeatureGate
    {
        private readonly HashSet<ContentAiTaskType> _allowed;

        public StubFeatureGate(bool enabled, IEnumerable<ContentAiTaskType>? allowed = null)
        {
            IsEnabled = enabled;
            _allowed = allowed is null
                ? Enum.GetValues<ContentAiTaskType>().ToHashSet()
                : allowed.ToHashSet();
        }

        public bool IsEnabled { get; }

        public string DefaultModel => "fake-v1";

        public bool IsTaskAllowed(ContentAiTaskType taskType) => _allowed.Contains(taskType);
    }

    private sealed class StubAiTextGenerator : IAiTextGenerator
    {
        private readonly AiTextResponse? _response;
        private readonly Exception? _failWith;

        public StubAiTextGenerator(AiTextResponse response)
        {
            _response = response;
        }

        public StubAiTextGenerator(Exception failWith)
        {
            _failWith = failWith;
        }

        public AiTextRequest? LastRequest { get; private set; }

        public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            if (_failWith is not null)
            {
                throw _failWith;
            }

            return Task.FromResult(_response!);
        }

        public Task<AiGenerationResult> GenerateSafeAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            if (_failWith is not null)
            {
                return Task.FromResult(AiGenerationResult.Fail(
                    AiErrorCodes.GenerationFailed,
                    1,
                    "Stub",
                    "stub"));
            }

            return Task.FromResult(AiGenerationResult.Ok(
                _response!.Text,
                1,
                _response.Model,
                _response.Provider,
                _response.Usage));
        }
    }

    private sealed class RecordingUsageRecorder : IAiUsageRecorder
    {
        public List<AiUsageRecordInput> Records { get; } = [];

        public Task RecordAsync(AiUsageRecordInput input, CancellationToken cancellationToken = default)
        {
            Records.Add(input);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingAuditRecorder : IAuditRecorder
    {
        public List<AuditRecordInput> Records { get; } = [];

        public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default)
        {
            Records.Add(input);
            return Task.CompletedTask;
        }
    }

    private sealed class StubContentService : IContentService
    {
        public AdminContentDetailDto DetailToReturn { get; set; } =
            new(
                Guid.Empty, "", "", "", "", null, "Article", "Draft", Guid.Empty, 0, 0,
                DateTime.UtcNow, DateTime.UtcNow, null,
                new SeoMetadataDto(null, null, null, null, null));

        public Exception? ExceptionToThrow { get; set; }

        public Task<IReadOnlyList<ContentListItemDto>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ContentDetailDto> GetPublishedBySlugAsync(
            string slug,
            Guid? viewerUserId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ContentDetailDto> CreateAsync(
            Guid authorId,
            CreateContentRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> UpdateAsync(
            ContentManagementActor actor,
            Guid id,
            UpdateContentRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> PublishAsync(
            ContentManagementActor actor,
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> UpdateSeoMetadataAsync(
            ContentManagementActor actor,
            Guid id,
            UpdateSeoMetadataRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> GetManagedByIdAsync(
            ContentManagementActor actor,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(DetailToReturn);
        }

        public Task<SeoAuditReportDto> AnalyzeSeoAsync(
            ContentManagementActor actor,
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public PreviewArticleDto Preview(PreviewArticleRequest request) =>
            throw new NotSupportedException();
    }
}
