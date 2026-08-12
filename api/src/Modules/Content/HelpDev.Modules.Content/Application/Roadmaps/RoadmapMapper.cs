using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
using HelpDev.Modules.Content.Domain.Roadmaps;

namespace HelpDev.Modules.Content.Application.Roadmaps;

internal static class RoadmapMapper
{
    public static RoadmapDetailDto ToDetail(RoadmapMetadata roadmap) =>
        new(
            roadmap.Id,
            roadmap.ContentId,
            roadmap.Level.ToString(),
            roadmap.EstimatedDuration,
            roadmap.Goal,
            roadmap.Prerequisites,
            roadmap.Steps
                .OrderBy(s => s.Order)
                .Select(ToStep)
                .ToList(),
            roadmap.CreatedAtUtc,
            roadmap.UpdatedAtUtc);

    public static RoadmapStepDto ToStep(RoadmapStep step) =>
        new(
            step.Id,
            step.Title,
            step.Description,
            step.Order,
            step.EstimatedHours,
            step.ProjectTitle,
            step.ProjectDescription,
            step.Topics.OrderBy(t => t.Order).Select(ToTopic).ToList(),
            step.Resources.OrderBy(r => r.Order).Select(ToResource).ToList());

    public static RoadmapTopicDto ToTopic(RoadmapTopic topic) =>
        new(topic.Id, topic.Title, topic.Description, topic.Order);

    public static RoadmapResourceDto ToResource(RoadmapResource resource) =>
        new(resource.Id, resource.Title, resource.Url, resource.ResourceType.ToString(), resource.Order);
}
