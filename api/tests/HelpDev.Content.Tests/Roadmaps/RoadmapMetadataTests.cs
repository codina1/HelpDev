using HelpDev.Modules.Content.Domain.Roadmaps;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests.Roadmaps;

public sealed class RoadmapMetadataTests
{
    [Fact]
    public void Create_requires_goal_and_duration()
    {
        Assert.Throws<DomainException>(() =>
            RoadmapMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                RoadmapLevel.Beginner,
                " ",
                "Goal",
                null,
                DateTime.UtcNow));

        Assert.Throws<DomainException>(() =>
            RoadmapMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                RoadmapLevel.Beginner,
                "8 weeks",
                " ",
                null,
                DateTime.UtcNow));
    }

    [Fact]
    public void Steps_reorder_requires_complete_unique_list()
    {
        var roadmap = RoadmapMetadata.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RoadmapLevel.Intermediate,
            "12 weeks",
            "Ship a frontend app",
            "HTML basics",
            DateTime.UtcNow);

        var a = roadmap.AddStep(Guid.NewGuid(), "HTML CSS", null, 0, 20, null, null, DateTime.UtcNow);
        var b = roadmap.AddStep(Guid.NewGuid(), "JavaScript", null, 1, 40, null, null, DateTime.UtcNow);
        var c = roadmap.AddStep(Guid.NewGuid(), "React", null, 2, 40, null, null, DateTime.UtcNow);

        roadmap.ReorderSteps([c.Id, a.Id, b.Id], DateTime.UtcNow);
        Assert.Equal(0, roadmap.GetRequiredStep(c.Id).Order);
        Assert.Equal(1, roadmap.GetRequiredStep(a.Id).Order);
        Assert.Equal(2, roadmap.GetRequiredStep(b.Id).Order);

        Assert.Throws<DomainException>(() => roadmap.ReorderSteps([a.Id, b.Id], DateTime.UtcNow));
        Assert.Throws<DomainException>(() => roadmap.ReorderSteps([a.Id, a.Id, b.Id], DateTime.UtcNow));
    }

    [Fact]
    public void Topics_and_resources_belong_to_step()
    {
        var step = RoadmapStep.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "JavaScript",
            "Core language",
            0,
            30,
            "Todo App",
            "Build a todo list");

        step.ReplaceTopics(
        [
            RoadmapTopic.Create(Guid.NewGuid(), step.Id, "Variables", null, 0),
            RoadmapTopic.Create(Guid.NewGuid(), step.Id, "Functions", null, 1),
            RoadmapTopic.Create(Guid.NewGuid(), step.Id, "Async", null, 2),
            RoadmapTopic.Create(Guid.NewGuid(), step.Id, "DOM", null, 3),
        ]);

        step.ReplaceResources(
        [
            RoadmapResource.Create(
                Guid.NewGuid(),
                step.Id,
                "MDN JS Guide",
                "https://developer.mozilla.org/",
                RoadmapResourceType.Article,
                0),
            RoadmapResource.Create(
                Guid.NewGuid(),
                step.Id,
                "Course link",
                "course:11111111-1111-1111-1111-111111111111",
                RoadmapResourceType.Course,
                1),
        ]);

        Assert.Equal(4, step.Topics.Count);
        Assert.Equal(2, step.Resources.Count);

        Assert.Throws<DomainException>(() =>
            step.ReplaceTopics(
            [
                RoadmapTopic.Create(Guid.NewGuid(), Guid.NewGuid(), "Wrong", null, 0),
            ]));
    }
}
