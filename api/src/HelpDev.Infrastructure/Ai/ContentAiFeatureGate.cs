using HelpDev.Modules.Content.Application.ContentAi;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

public sealed class ContentAiFeatureGate : IContentAiFeatureGate
{
    private readonly AiProviderOptions _options;
    private readonly HashSet<string> _allowed;

    public ContentAiFeatureGate(IOptions<AiProviderOptions> options)
    {
        _options = options.Value;
        _allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in (_options.AllowedTasks ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            _allowed.Add(part);
        }

        if (_allowed.Count == 0)
        {
            foreach (ContentAiTaskType task in Enum.GetValues<ContentAiTaskType>())
            {
                _allowed.Add(task.ToString());
            }
        }
    }

    public bool IsEnabled => _options.Enabled;

    public string DefaultModel => _options.Model;

    public bool IsTaskAllowed(ContentAiTaskType taskType) =>
        _allowed.Contains(taskType.ToString());
}
