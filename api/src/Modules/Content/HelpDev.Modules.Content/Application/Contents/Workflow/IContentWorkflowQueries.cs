using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;

namespace HelpDev.Modules.Content.Application.Contents.Workflow;

public interface IContentWorkflowQueries
{
    Task<WorkflowHistoryDto> GetHistoryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);
}
