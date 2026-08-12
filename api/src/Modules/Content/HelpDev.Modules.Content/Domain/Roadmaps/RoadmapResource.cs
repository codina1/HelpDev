using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Roadmaps;

public sealed class RoadmapResource
{
    public const int MaxTitleLength = 200;
    public const int MaxUrlLength = 2048;

    private RoadmapResource()
    {
    }

    private RoadmapResource(
        Guid id,
        Guid stepId,
        string title,
        string url,
        RoadmapResourceType resourceType,
        int order)
    {
        Id = id;
        StepId = stepId;
        Title = title;
        Url = url;
        ResourceType = resourceType;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid StepId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public RoadmapResourceType ResourceType { get; private set; }

    public int Order { get; private set; }

    public static RoadmapResource Create(
        Guid id,
        Guid stepId,
        string title,
        string url,
        RoadmapResourceType resourceType,
        int order)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه منبع الزامی است.");
        }

        if (stepId == Guid.Empty)
        {
            throw new DomainException("شناسه گام الزامی است.");
        }

        if (order < 0)
        {
            throw new DomainException("ترتیب منبع نمی‌تواند منفی باشد.");
        }

        if (!Enum.IsDefined(resourceType))
        {
            throw new DomainException("نوع منبع معتبر نیست.");
        }

        return new RoadmapResource(
            id,
            stepId,
            NormalizeTitle(title),
            NormalizeUrl(url),
            resourceType,
            order);
    }

    public void Update(string title, string url, RoadmapResourceType resourceType, int order)
    {
        var updated = Create(Id, StepId, title, url, resourceType, order);
        Title = updated.Title;
        Url = updated.Url;
        ResourceType = updated.ResourceType;
        Order = updated.Order;
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("عنوان منبع الزامی است.");
        }

        var normalized = title.Trim();
        if (normalized.Length > MaxTitleLength)
        {
            throw new DomainException("عنوان منبع بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new DomainException("آدرس منبع الزامی است.");
        }

        var normalized = url.Trim();
        if (normalized.Length > MaxUrlLength)
        {
            throw new DomainException("آدرس منبع بیش از حد مجاز است.");
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            // Allow opaque identifiers (content:/tool:/course:) for cross-module links without hard FKs.
            if (normalized.StartsWith("content:", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("tool:", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("course:", StringComparison.OrdinalIgnoreCase)
                || Guid.TryParse(normalized, out _))
            {
                return normalized;
            }

            throw new DomainException("آدرس منبع معتبر نیست.");
        }

        return normalized;
    }
}
