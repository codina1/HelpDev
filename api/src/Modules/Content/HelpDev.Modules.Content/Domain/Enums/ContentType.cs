namespace HelpDev.Modules.Content.Domain.Enums;

public enum ContentType
{
    News = 0,
    Article = 1,
    RoadmapStep = 2,
    Tool = 3,
    Prompt = 4,
    Course = 5,
    /// <summary>Parent content for Roadmap Engine (phases/steps live in satellite tables).</summary>
    Roadmap = 6,
}
