using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptRenderRecordTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_bounds_preview_length()
    {
        var oversizedInput = new string('a', PromptLabLimits.MaxHistoryInputPreview + 50);
        var oversizedOutput = new string('b', PromptLabLimits.MaxHistoryOutputPreview + 50);

        var record = PromptRenderRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            versionNumber: 1,
            Guid.NewGuid(),
            succeeded: true,
            durationMilliseconds: 12,
            oversizedInput,
            oversizedOutput,
            errorCode: null,
            Now);

        Assert.Equal(PromptLabLimits.MaxHistoryInputPreview, record.InputPreview!.Length);
        Assert.Equal(PromptLabLimits.MaxHistoryOutputPreview, record.RenderedPreview!.Length);
    }

    [Fact]
    public void Create_rejects_negative_duration()
    {
        var ex = Assert.Throws<DomainException>(() =>
            PromptRenderRecord.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                versionNumber: 1,
                Guid.NewGuid(),
                succeeded: true,
                durationMilliseconds: -1,
                inputPreview: null,
                renderedPreview: null,
                errorCode: null,
                Now));

        Assert.Equal(PromptLabErrorCodes.RenderFailed, ex.Code);
    }

    [Fact]
    public void Create_redacts_sensitive_tokens_in_preview()
    {
        var record = PromptRenderRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            true,
            1,
            "password=secret token=abc",
            "apikey=xyz",
            null,
            Now);

        Assert.DoesNotContain("password", record.InputPreview!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("***", record.InputPreview!, StringComparison.Ordinal);
        Assert.Contains("***", record.RenderedPreview!, StringComparison.Ordinal);
    }
}
