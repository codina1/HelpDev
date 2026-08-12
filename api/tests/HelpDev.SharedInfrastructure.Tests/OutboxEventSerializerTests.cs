using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Tests;

public sealed class OutboxEventRegistryAndSerializerTests
{
    [Fact]
    public void Registry_rejects_duplicate_stable_name_and_clr_type()
    {
        var registry = new OutboxEventTypeRegistry();
        registry.Register<ContentPublishedDomainEvent>("content.published.v1");

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register<ContentUpdatedDomainEvent>("content.published.v1"));
        Assert.Throws<InvalidOperationException>(() =>
            registry.Register<ContentPublishedDomainEvent>("content.published.v2"));
    }

    [Fact]
    public void Registry_rejects_unknown_type_and_name()
    {
        var registry = CreateProductionRegistry();

        Assert.Throws<InvalidOperationException>(() =>
            registry.GetStableName(typeof(UnregisteredEvent)));
        Assert.Throws<InvalidOperationException>(() =>
            registry.GetClrType("unknown.event.v1"));
        Assert.False(registry.TryGetClrType("System.Object", out _));
    }

    [Fact]
    public void Registry_seals_against_further_registration()
    {
        var registry = new OutboxEventTypeRegistry();
        registry.Register<ContentPublishedDomainEvent>("content.published.v1");
        registry.Seal();

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register<ContentUpdatedDomainEvent>("content.updated.v1"));
    }

    [Theory]
    [MemberData(nameof(ProductionEvents))]
    public void Serialize_deserialize_round_trips_production_events(IDomainEvent domainEvent, string expectedType)
    {
        var serializer = new SystemTextJsonOutboxEventSerializer(CreateProductionRegistry());

        var serialized = serializer.Serialize(domainEvent);
        Assert.Equal(expectedType, serialized.Type);
        Assert.Equal(domainEvent.EventId, serialized.Id);
        Assert.Equal(domainEvent.OccurredAtUtc, serialized.OccurredAtUtc);
        Assert.DoesNotContain("HelpDev.Modules", serialized.Type, StringComparison.Ordinal);

        var restored = serializer.Deserialize(serialized.Type, serialized.Payload);
        Assert.Equal(domainEvent.GetType(), restored.GetType());
        Assert.Equal(domainEvent.EventId, restored.EventId);
    }

    [Fact]
    public void Deserialize_rejects_malformed_payload()
    {
        var serializer = new SystemTextJsonOutboxEventSerializer(CreateProductionRegistry());

        Assert.Throws<InvalidOperationException>(() =>
            serializer.Deserialize("content.published.v1", "{not-json"));
    }

    [Fact]
    public void Deserialize_does_not_load_arbitrary_type_names()
    {
        var serializer = new SystemTextJsonOutboxEventSerializer(CreateProductionRegistry());

        Assert.Throws<InvalidOperationException>(() =>
            serializer.Deserialize("System.IO.FileInfo, System.IO", "{}"));
    }

    [Fact]
    public void Production_registry_contains_all_current_domain_events()
    {
        var registry = CreateProductionRegistry();
        var expected = new HashSet<Type>
        {
            typeof(ContentPublishedDomainEvent),
            typeof(ContentUpdatedDomainEvent),
            typeof(CoursePublishedDomainEvent),
            typeof(CourseUpdatedDomainEvent),
            typeof(LessonPublishedDomainEvent),
            typeof(StudentEnrolledDomainEvent),
            typeof(LessonCompletedDomainEvent),
            typeof(CourseCompletedDomainEvent),
            typeof(ToolPublishedDomainEvent),
            typeof(ToolUnpublishedDomainEvent),
            typeof(PromptPublishedDomainEvent),
            typeof(PromptUnpublishedDomainEvent),
        };

        Assert.Equal(expected, registry.RegisteredEventTypes.ToHashSet());
        Assert.Equal("learning.course-updated.v1", registry.GetStableName(typeof(CourseUpdatedDomainEvent)));
        Assert.Equal(typeof(CourseUpdatedDomainEvent), registry.GetClrType("learning.course-updated.v1"));
    }

    public static IEnumerable<object[]> ProductionEvents()
    {
        yield return
        [
            new ContentPublishedDomainEvent(Guid.NewGuid(), "slug-a"),
            "content.published.v1",
        ];
        yield return
        [
            new ContentUpdatedDomainEvent(Guid.NewGuid(), "slug-b"),
            "content.updated.v1",
        ];
        yield return
        [
            new CoursePublishedDomainEvent(Guid.NewGuid(), "course-slug"),
            "learning.course-published.v1",
        ];
        yield return
        [
            new CourseUpdatedDomainEvent(Guid.NewGuid()),
            "learning.course-updated.v1",
        ];
        yield return
        [
            new LessonPublishedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "course-slug"),
            "learning.lesson-published.v1",
        ];
        yield return
        [
            new StudentEnrolledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            "learning.student-enrolled.v1",
        ];
        yield return
        [
            new LessonCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            "learning.lesson-completed.v1",
        ];
        yield return
        [
            new CourseCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            "learning.course-completed.v1",
        ];
        yield return
        [
            new ToolPublishedDomainEvent(Guid.NewGuid(), "tool-slug"),
            "toolbox.tool-published.v1",
        ];
        yield return
        [
            new ToolUnpublishedDomainEvent(Guid.NewGuid(), "tool-slug"),
            "toolbox.tool-unpublished.v1",
        ];
        yield return
        [
            new PromptPublishedDomainEvent(Guid.NewGuid(), "prompt-slug", 1),
            "promptlab.prompt-published.v1",
        ];
        yield return
        [
            new PromptUnpublishedDomainEvent(Guid.NewGuid(), "prompt-slug"),
            "promptlab.prompt-unpublished.v1",
        ];
    }

    private static OutboxEventTypeRegistry CreateProductionRegistry()
    {
        var registry = new OutboxEventTypeRegistry();
        registry.Register<ContentPublishedDomainEvent>("content.published.v1");
        registry.Register<ContentUpdatedDomainEvent>("content.updated.v1");
        registry.Register<CoursePublishedDomainEvent>("learning.course-published.v1");
        registry.Register<CourseUpdatedDomainEvent>("learning.course-updated.v1");
        registry.Register<LessonPublishedDomainEvent>("learning.lesson-published.v1");
        registry.Register<StudentEnrolledDomainEvent>("learning.student-enrolled.v1");
        registry.Register<LessonCompletedDomainEvent>("learning.lesson-completed.v1");
        registry.Register<CourseCompletedDomainEvent>("learning.course-completed.v1");
        registry.Register<ToolPublishedDomainEvent>("toolbox.tool-published.v1");
        registry.Register<ToolUnpublishedDomainEvent>("toolbox.tool-unpublished.v1");
        registry.Register<PromptPublishedDomainEvent>("promptlab.prompt-published.v1");
        registry.Register<PromptUnpublishedDomainEvent>("promptlab.prompt-unpublished.v1");
        registry.Seal();
        return registry;
    }

    private sealed record UnregisteredEvent(string Name) : DomainEvent;
}
