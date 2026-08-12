using HelpDev.Modules.Analytics.Application;
using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Analytics.Tests;

public sealed class AnalyticsEventValidatorTests
{
    private static AnalyticsEventEnvelope ValidEnvelope(
        string? eventType = null,
        IReadOnlyDictionary<string, string>? dimensions = null) =>
        new(
            EventId: Guid.NewGuid(),
            EventType: eventType ?? AnalyticsEventTypes.IdentityUserLoginSucceeded,
            OccurredAtUtc: new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            ActorUserId: Guid.NewGuid(),
            SubjectId: null,
            SubjectType: null,
            Dimensions: dimensions,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

    [Fact]
    public void Valid_envelope_passes_without_exception()
    {
        AnalyticsEventValidator.Validate(ValidEnvelope());
    }

    [Fact]
    public void Empty_event_id_throws()
    {
        var envelope = ValidEnvelope() with { EventId = Guid.Empty };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventIdRequired, ex.Code);
    }

    [Fact]
    public void Blank_event_type_throws()
    {
        var envelope = ValidEnvelope() with { EventType = "  " };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventTypeRequired, ex.Code);
    }

    [Fact]
    public void Unsupported_event_type_throws()
    {
        var envelope = ValidEnvelope() with { EventType = "unknown.event_type" };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventTypeUnsupported, ex.Code);
    }

    [Fact]
    public void Default_timestamp_throws()
    {
        var envelope = ValidEnvelope() with { OccurredAtUtc = default };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventTimestampInvalid, ex.Code);
    }

    [Fact]
    public void Non_utc_timestamp_throws()
    {
        var envelope = ValidEnvelope() with
        {
            OccurredAtUtc = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
        };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventTimestampInvalid, ex.Code);
    }

    [Fact]
    public void Schema_version_other_than_1_throws()
    {
        var envelope = ValidEnvelope() with { SchemaVersion = 2 };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventSchemaVersionUnsupported, ex.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Invalid_quantity_throws(long quantity)
    {
        var envelope = ValidEnvelope() with { Quantity = quantity };

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventQuantityInvalid, ex.Code);
    }

    [Fact]
    public void Max_quantity_1000_passes()
    {
        var envelope = ValidEnvelope() with { Quantity = 1000 };
        AnalyticsEventValidator.Validate(envelope);
    }

    [Fact]
    public void Dimension_key_starting_with_uppercase_throws()
    {
        // Keys must match ^[a-z][a-z0-9_]*$
        var dims = new Dictionary<string, string> { ["Purpose"] = "codeReview" };
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dims);

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Fact]
    public void Disallowed_lowercase_dimension_key_throws()
    {
        // "purpose" is a valid key for PromptLabRenderSucceeded, but "not_allowed_key" is not
        var dims = new Dictionary<string, string>
        {
            ["purpose"] = "codeReview",
            ["not_allowed_key"] = "value",
        };
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dims);

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionNotAllowed, ex.Code);
    }

    [Fact]
    public void Dimension_value_with_url_throws()
    {
        var dims = new Dictionary<string, string>
        {
            ["purpose"] = "https://evil.com/leak",
        };
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dims);

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Fact]
    public void Dimension_value_with_email_throws()
    {
        var dims = new Dictionary<string, string>
        {
            ["purpose"] = "user@example.com",
        };
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dims);

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Fact]
    public void Missing_required_dimension_for_promptlab_event_throws()
    {
        // PromptLabRenderSucceeded requires "purpose" dimension
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dimensions: null);

        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Fact]
    public void Purpose_dimension_alone_passes_format_check_but_fails_required_check()
    {
        // "purpose" is required by PromptLabRenderSucceeded, but "isAuthenticated" is also required
        // Since "isAuthenticated" has uppercase, camelCase keys fail the regex.
        // Test documents that all-lowercase "purpose" passes format; isAuthenticated (camelCase) does not.
        var dimsWithOnlyPurpose = new Dictionary<string, string>
        {
            ["purpose"] = "codeReview",
        };
        var envelope = ValidEnvelope(AnalyticsEventTypes.PromptLabRenderSucceeded, dimsWithOnlyPurpose);

        // Missing isAuthenticated which is a required dimension (even though it has uppercase)
        // The validator will throw because required key "isAuthenticated" is absent
        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Theory]
    [InlineData(AnalyticsEventTypes.IdentityUserLoginSucceeded)]
    [InlineData(AnalyticsEventTypes.LearningCourseCreated)]
    [InlineData(AnalyticsEventTypes.LearningCoursePublished)]
    [InlineData(AnalyticsEventTypes.LearningEnrollmentCreated)]
    [InlineData(AnalyticsEventTypes.LearningLessonCompleted)]
    public void Event_types_with_no_required_dimensions_pass_without_dimensions(string eventType)
    {
        var envelope = ValidEnvelope(eventType, dimensions: null);
        AnalyticsEventValidator.Validate(envelope);
    }

    [Fact]
    public void IdentityUserRegistered_requires_registrationmethod_dimension()
    {
        // registrationMethod (camelCase) is required but fails format regex
        // Without it the validator throws due to missing required dimension
        var envelope = ValidEnvelope(AnalyticsEventTypes.IdentityUserRegistered, dimensions: null);
        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }

    [Fact]
    public void SearchDocumentIndexed_requires_sourcetype_dimension()
    {
        var envelope = ValidEnvelope(AnalyticsEventTypes.SearchDocumentIndexed, dimensions: null);
        var ex = Assert.Throws<AnalyticsException>(() => AnalyticsEventValidator.Validate(envelope));
        Assert.Equal(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, ex.Code);
    }
}
