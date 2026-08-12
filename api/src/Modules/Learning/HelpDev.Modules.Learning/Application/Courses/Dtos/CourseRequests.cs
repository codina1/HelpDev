namespace HelpDev.Modules.Learning.Application.Courses.Dtos;

public sealed class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class AddSectionRequest
{
    public string Title { get; set; } = string.Empty;
}

public sealed class RenameSectionRequest
{
    public Guid SectionId { get; set; }

    public string Title { get; set; } = string.Empty;
}

public sealed class ReorderSectionRequest
{
    public Guid SectionId { get; set; }

    public int NewOrder { get; set; }
}

public sealed class AddLessonRequest
{
    public Guid SectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid? ContentId { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsPreview { get; set; }
}

public sealed class UpdateLessonRequest
{
    public Guid SectionId { get; set; }

    public Guid LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid? ContentId { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsPreview { get; set; }
}

public sealed class ReorderLessonRequest
{
    public Guid SectionId { get; set; }

    public Guid LessonId { get; set; }

    public int NewOrder { get; set; }
}

/// <summary>HTTP body for rename where SectionId comes from the route.</summary>
public sealed class RenameSectionBody
{
    public string Title { get; set; } = string.Empty;
}

/// <summary>HTTP body for reorder where ids come from the route.</summary>
public sealed class ReorderBody
{
    public int NewOrder { get; set; }
}

/// <summary>HTTP body for add lesson where SectionId comes from the route.</summary>
public sealed class AddLessonBody
{
    public string Title { get; set; } = string.Empty;

    public Guid? ContentId { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsPreview { get; set; }
}

/// <summary>HTTP body for update lesson where ids come from the route.</summary>
public sealed class UpdateLessonBody
{
    public string Title { get; set; } = string.Empty;

    public Guid? ContentId { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsPreview { get; set; }
}
