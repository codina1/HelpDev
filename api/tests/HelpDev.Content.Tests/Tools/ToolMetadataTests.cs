using HelpDev.Modules.Content.Domain.Tools;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests.Tools;

public sealed class ToolMetadataTests
{
    [Fact]
    public void Create_requires_name_and_website()
    {
        Assert.Throws<DomainException>(() =>
            ToolMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                " ",
                "https://example.com",
                null,
                null,
                null,
                PricingModel.Free,
                "IDE",
                PlatformSupport.Web,
                LicenseType.Commercial,
                DateTime.UtcNow));

        Assert.Throws<DomainException>(() =>
            ToolMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Cursor",
                "not-a-url",
                null,
                null,
                null,
                PricingModel.Free,
                "IDE",
                PlatformSupport.Web,
                LicenseType.Commercial,
                DateTime.UtcNow));
    }

    [Fact]
    public void Features_and_alternatives_are_independent_of_content_lifecycle()
    {
        var contentId = Guid.NewGuid();
        var tool = ToolMetadata.Create(
            Guid.NewGuid(),
            contentId,
            "Cursor",
            "https://cursor.com",
            "https://github.com/cursor",
            null,
            "Anysphere",
            PricingModel.Freemium,
            "IDE",
            PlatformSupport.Windows | PlatformSupport.MacOS | PlatformSupport.Web,
            LicenseType.Commercial,
            DateTime.UtcNow);

        tool.AddFeature(Guid.NewGuid(), "AI Agent", "Agentic editing", 0, DateTime.UtcNow);
        tool.AddFeature(Guid.NewGuid(), "Composer", null, 1, DateTime.UtcNow);
        tool.ReplaceAlternatives(
            [
                ToolAlternative.Create(Guid.NewGuid(), tool.Id, Guid.NewGuid(), 0),
                ToolAlternative.Create(Guid.NewGuid(), tool.Id, Guid.NewGuid(), 1),
            ],
            DateTime.UtcNow);

        Assert.Equal(2, tool.Features.Count);
        Assert.Equal(2, tool.Alternatives.Count);
        Assert.Equal(contentId, tool.ContentId);

        var featureId = tool.Features.First().Id;
        tool.RemoveFeature(featureId, DateTime.UtcNow);
        Assert.Single(tool.Features);
    }

    [Fact]
    public void Cannot_add_self_as_alternative()
    {
        var contentId = Guid.NewGuid();
        var tool = ToolMetadata.Create(
            Guid.NewGuid(),
            contentId,
            "Cursor",
            "https://cursor.com",
            null,
            null,
            null,
            PricingModel.Free,
            "IDE",
            PlatformSupport.Web,
            LicenseType.OpenSource,
            DateTime.UtcNow);

        Assert.Throws<DomainException>(() =>
            tool.ReplaceAlternatives(
                [ToolAlternative.Create(Guid.NewGuid(), tool.Id, contentId, 0)],
                DateTime.UtcNow));
    }
}
