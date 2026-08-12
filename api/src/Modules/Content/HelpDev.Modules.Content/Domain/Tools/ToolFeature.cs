using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Tools;

public sealed class ToolFeature
{
    public const int MaxTitleLength = 160;
    public const int MaxDescriptionLength = 1000;

    private ToolFeature()
    {
    }

    private ToolFeature(Guid id, Guid toolId, string title, string? description, int order)
    {
        Id = id;
        ToolId = toolId;
        Title = title;
        Description = description;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid ToolId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public static ToolFeature Create(Guid id, Guid toolId, string title, string? description, int order)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه ویژگی الزامی است.");
        }

        if (toolId == Guid.Empty)
        {
            throw new DomainException("شناسه ابزار الزامی است.");
        }

        if (order < 0)
        {
            throw new DomainException("ترتیب ویژگی نمی‌تواند منفی باشد.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("عنوان ویژگی الزامی است.");
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > MaxTitleLength)
        {
            throw new DomainException("عنوان ویژگی بیش از حد مجاز است.");
        }

        string? normalizedDescription = null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            normalizedDescription = description.Trim();
            if (normalizedDescription.Length > MaxDescriptionLength)
            {
                throw new DomainException("توضیح ویژگی بیش از حد مجاز است.");
            }
        }

        return new ToolFeature(id, toolId, normalizedTitle, normalizedDescription, order);
    }

    public void Update(string title, string? description, int order)
    {
        var updated = Create(Id, ToolId, title, description, order);
        Title = updated.Title;
        Description = updated.Description;
        Order = updated.Order;
    }
}
