using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Toolbox.Application.Tests.Fakes;

namespace HelpDev.Toolbox.Application.Tests;

public sealed class ToolExecutionServiceTests
{
    private readonly FakeToolDefinitionRepository _toolRepository = new();
    private readonly FakeToolExecutionRecordRepository _executionRepository = new();
    private readonly StubToolExecutor _executor = new(ToolType.TextStatistics);
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly ToolExecutionService _sut;

    public ToolExecutionServiceTests()
    {
        _sut = ServiceFactory.CreateExecutionService(
            _toolRepository,
            _executionRepository,
            new FakeExecutorRegistry(_executor),
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task Execute_success_without_history_does_not_commit()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow, allowHistory: false);
        _toolRepository.Seed(tool);

        using var document = JsonDocument.Parse("""{"text":"hi"}""");
        var result = await _sut.ExecuteAsync(
            tool.Slug.Value,
            new ExecuteToolRequest(document.RootElement.Clone()));

        Assert.True(result.Succeeded);
        Assert.Null(result.ExecutionId);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(0, _executionRepository.AddCallCount);
    }

    [Fact]
    public async Task Execute_with_allow_history_and_user_commits_once()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow, allowHistory: true);
        _toolRepository.Seed(tool);
        var userId = Guid.NewGuid();

        using var document = JsonDocument.Parse("""{"text":"hi"}""");
        using var cts = new CancellationTokenSource();

        var result = await _sut.ExecuteAsync(
            tool.Slug.Value,
            new ExecuteToolRequest(document.RootElement.Clone()),
            userId,
            cts.Token);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ExecutionId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(1, _executionRepository.AddCallCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
        Assert.Equal(cts.Token, _executionRepository.LastToken);
        Assert.Equal(cts.Token, _executor.LastToken);
    }

    [Fact]
    public async Task Execute_requires_auth_for_anonymous_user()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow, requiresAuthentication: true);
        _toolRepository.Seed(tool);

        using var document = JsonDocument.Parse("""{"text":"hi"}""");
        var ex = await Assert.ThrowsAsync<ToolboxException>(() =>
            _sut.ExecuteAsync(tool.Slug.Value, new ExecuteToolRequest(document.RootElement.Clone())));

        Assert.Equal(ToolboxApplicationErrorCodes.ToolRequiresAuthentication, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Execute_unpublished_throws_tool_not_found()
    {
        var tool = ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hidden",
            "hidden-tool",
            "Hidden",
            null,
            ToolType.TextStatistics,
            ServiceFactory.DefaultSchema,
            null,
            false,
            false,
            0,
            _clock.UtcNow);
        _toolRepository.Seed(tool);

        using var document = JsonDocument.Parse("""{"text":"hi"}""");
        var ex = await Assert.ThrowsAsync<ToolboxException>(() =>
            _sut.ExecuteAsync(tool.Slug.Value, new ExecuteToolRequest(document.RootElement.Clone())));

        Assert.Equal(ToolboxApplicationErrorCodes.ToolNotFound, ex.Code);
    }

    [Fact]
    public async Task Execute_oversized_input_throws()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow);
        _toolRepository.Seed(tool);

        var oversized = new string('x', ToolboxLimits.MaxJsonLength + 1);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new { text = oversized }));
        var ex = await Assert.ThrowsAsync<ToolboxException>(() =>
            _sut.ExecuteAsync(tool.Slug.Value, new ExecuteToolRequest(document.RootElement.Clone())));

        Assert.Equal(ToolboxApplicationErrorCodes.ExecutionInputTooLarge, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
