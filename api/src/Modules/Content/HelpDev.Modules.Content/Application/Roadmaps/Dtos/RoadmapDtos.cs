namespace HelpDev.Modules.Content.Application.Roadmaps.Dtos;

public sealed record RoadmapTopicDto(
    Guid Id,
    string Title,
    string? Description,
    int Order);

public sealed record RoadmapResourceDto(
    Guid Id,
    string Title,
    string Url,
    string ResourceType,
    int Order);

public sealed record RoadmapStepDto(
    Guid Id,
    string Title,
    string? Description,
    int Order,
    int EstimatedHours,
    string? ProjectTitle,
    string? ProjectDescription,
    IReadOnlyList<RoadmapTopicDto> Topics,
    IReadOnlyList<RoadmapResourceDto> Resources);

public sealed record RoadmapDetailDto(
    Guid Id,
    Guid ContentId,
    string Level,
    string EstimatedDuration,
    string Goal,
    string? Prerequisites,
    IReadOnlyList<RoadmapStepDto> Steps,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record RoadmapListItemDto(
    Guid Id,
    Guid ContentId,
    string Level,
    string EstimatedDuration,
    string ContentSlug,
    string ContentStatus,
    string ContentTitle,
    DateTime UpdatedAtUtc);

public sealed class UpdateRoadmapRequest
{
    public string Level { get; set; } = nameof(Domain.Roadmaps.RoadmapLevel.Beginner);

    public string EstimatedDuration { get; set; } = string.Empty;

    public string Goal { get; set; } = string.Empty;

    public string? Prerequisites { get; set; }
}

public sealed class CreateRoadmapStepRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Order { get; set; }

    public int EstimatedHours { get; set; }

    public string? ProjectTitle { get; set; }

    public string? ProjectDescription { get; set; }

    public IReadOnlyList<UpsertRoadmapTopicItem>? Topics { get; set; }

    public IReadOnlyList<UpsertRoadmapResourceItem>? Resources { get; set; }
}

public sealed class UpdateRoadmapStepRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public int EstimatedHours { get; set; }

    public string? ProjectTitle { get; set; }

    public string? ProjectDescription { get; set; }

    public IReadOnlyList<UpsertRoadmapTopicItem>? Topics { get; set; }

    public IReadOnlyList<UpsertRoadmapResourceItem>? Resources { get; set; }
}

public sealed class UpsertRoadmapTopicItem
{
    public Guid? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }
}

public sealed class UpsertRoadmapResourceItem
{
    public Guid? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ResourceType { get; set; } = nameof(Domain.Roadmaps.RoadmapResourceType.External);

    public int Order { get; set; }
}

public sealed class ReorderRoadmapStepsRequest
{
    public IReadOnlyList<Guid> StepIds { get; set; } = [];
}
