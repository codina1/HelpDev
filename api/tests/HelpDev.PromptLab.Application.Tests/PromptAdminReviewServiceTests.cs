using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.PromptLab.Application.Tests.Fakes;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptAdminReviewServiceTests
{
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid AdminId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private readonly FakePromptRepository _prompts = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptAdminReviewService _sut;

    public PromptAdminReviewServiceTests()
    {
        _sut = new PromptAdminReviewService(
            _prompts,
            new FakePromptAdminReviewQueries(_prompts),
            _unitOfWork,
            _clock,
            new NoOpAuditRecorder(),
            new FakeAuditRequestContext(),
            NullLogger<PromptAdminReviewService>.Instance);
    }

    [Fact]
    public async Task Approve_publishes_submitted_prompt()
    {
        var prompt = SeedSubmitted();

        var dto = await _sut.ApproveAsync(AdminId, prompt.Id);

        Assert.Equal(nameof(PromptStatus.Approved), dto.Status);
        Assert.Equal(PromptStatus.Approved, prompt.Status);
        Assert.NotNull(prompt.PublishedAt);
        Assert.True(prompt.IsPublic);
        Assert.Null(prompt.RejectionReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Reject_requires_reason_and_is_not_public()
    {
        var prompt = SeedSubmitted();

        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => _sut.RejectAsync(AdminId, prompt.Id, new RejectAdminPromptRequest("  ")));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptRejectionReasonRequired, missing.Code);
        Assert.Equal(PromptStatus.Submitted, prompt.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);

        var dto = await _sut.RejectAsync(AdminId, prompt.Id, new RejectAdminPromptRequest("عنوان مبهم است."));

        Assert.Equal(nameof(PromptStatus.Rejected), dto.Status);
        Assert.Equal("عنوان مبهم است.", dto.RejectionReason);
        Assert.False(prompt.IsPublic);
        Assert.Null(prompt.PublishedAt);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Approve_missing_prompt_is_not_found()
    {
        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => _sut.ApproveAsync(AdminId, Guid.NewGuid()));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, missing.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    private Prompt SeedSubmitted()
    {
        var utc = _clock.UtcNow;
        var prompt = Prompt.Create(
            Guid.NewGuid(),
            "Review helper",
            "review-helper",
            "Helps review code",
            "Review {{code}}",
            null,
            PromptMediaType.Text,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            AuthorId,
            utc);
        prompt.Submit(AuthorId, utc.AddMinutes(1));
        _prompts.Seed(prompt);
        return prompt;
    }

    private sealed class FakePromptAdminReviewQueries : IPromptAdminReviewQueries
    {
        private readonly FakePromptRepository _prompts;

        public FakePromptAdminReviewQueries(FakePromptRepository prompts) => _prompts = prompts;

        public Task<AdminPromptReviewPageDto> GetPromptsAsync(
            AdminPromptReviewFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdminPromptReviewPageDto(1, 20, 0, []));

        public Task<AdminPromptReviewDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var prompt = _prompts.Items.FirstOrDefault(item => item.Id == id);
            if (prompt is null)
            {
                return Task.FromResult<AdminPromptReviewDetailsDto?>(null);
            }

            return Task.FromResult<AdminPromptReviewDetailsDto?>(new AdminPromptReviewDetailsDto(
                prompt.Id,
                prompt.Title,
                prompt.Slug.Value,
                prompt.Description,
                prompt.Content,
                prompt.CoverImage,
                prompt.MediaType.ToString(),
                prompt.AuthorId,
                prompt.CategoryId,
                "Coding",
                prompt.AiModelId,
                prompt.Status.ToString(),
                prompt.RejectionReason,
                prompt.Views,
                prompt.CopyCount,
                prompt.CreatedAt,
                prompt.UpdatedAt,
                prompt.PublishedAt));
        }
    }
}
