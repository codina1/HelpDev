using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Toolbox.Tests;

public sealed class ToolExecutionRecordTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_bounds_preview_length()
    {
        var oversized = new string('a', ToolboxLimits.MaxHistoryInputPreview + 50);

        var record = ToolExecutionRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ToolType.TextStatistics,
            succeeded: true,
            durationMilliseconds: 12,
            oversized,
            oversized,
            errorCode: null,
            Now);

        Assert.Equal(ToolboxLimits.MaxHistoryInputPreview, record.InputPreview!.Length);
        Assert.Equal(ToolboxLimits.MaxHistoryInputPreview, record.OutputPreview!.Length);
    }

    [Fact]
    public void Create_rejects_negative_duration()
    {
        var ex = Assert.Throws<DomainException>(() =>
            ToolExecutionRecord.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ToolType.JsonFormatter,
                succeeded: true,
                durationMilliseconds: -1,
                inputPreview: null,
                outputPreview: null,
                errorCode: null,
                Now));

        Assert.Equal(ToolboxErrorCodes.ExecutionFailed, ex.Code);
    }
}
