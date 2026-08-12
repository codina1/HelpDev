using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Tools;

public sealed class ToolAlternative
{
    private ToolAlternative()
    {
    }

    private ToolAlternative(Guid id, Guid toolId, Guid alternativeToolContentId, int order)
    {
        Id = id;
        ToolId = toolId;
        AlternativeToolContentId = alternativeToolContentId;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid ToolId { get; private set; }

    public Guid AlternativeToolContentId { get; private set; }

    public int Order { get; private set; }

    public static ToolAlternative Create(Guid id, Guid toolId, Guid alternativeToolContentId, int order)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه جایگزین الزامی است.");
        }

        if (toolId == Guid.Empty)
        {
            throw new DomainException("شناسه ابزار الزامی است.");
        }

        if (alternativeToolContentId == Guid.Empty)
        {
            throw new DomainException("شناسه محتوای جایگزین الزامی است.");
        }

        if (order < 0)
        {
            throw new DomainException("ترتیب جایگزین نمی‌تواند منفی باشد.");
        }

        return new ToolAlternative(id, toolId, alternativeToolContentId, order);
    }
}
