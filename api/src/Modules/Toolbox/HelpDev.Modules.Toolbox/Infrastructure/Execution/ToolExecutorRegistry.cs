using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

public sealed class ToolExecutorRegistry : IToolExecutorRegistry
{
    private readonly IReadOnlyDictionary<ToolType, IToolExecutor> _executors;

    public ToolExecutorRegistry(IEnumerable<IToolExecutor> executors)
    {
        ArgumentNullException.ThrowIfNull(executors);

        var map = new Dictionary<ToolType, IToolExecutor>();
        foreach (var executor in executors)
        {
            ArgumentNullException.ThrowIfNull(executor);
            if (!map.TryAdd(executor.Type, executor))
            {
                throw new InvalidOperationException(
                    $"Duplicate tool executor registration for {executor.Type}.");
            }
        }

        _executors = map;
    }

    public IToolExecutor GetRequired(ToolType type)
    {
        if (_executors.TryGetValue(type, out var executor))
        {
            return executor;
        }

        throw new ToolboxException(
            "Tool type is not supported.",
            ToolboxApplicationErrorCodes.ExecutionTypeUnsupported);
    }
}
