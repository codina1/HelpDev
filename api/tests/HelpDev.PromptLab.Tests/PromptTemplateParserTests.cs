using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptTemplateParserTests
{
    private readonly PromptTemplateParser _sut = new();

    [Fact]
    public void Extract_returns_placeholders_in_first_occurrence_order()
    {
        var names = _sut.ExtractPlaceholders("Hello {{name}}, review {{code}} then {{name}} again.");

        Assert.Equal(["name", "code"], names);
    }

    [Fact]
    public void Extract_duplicates_collapse_to_first_occurrence()
    {
        var names = _sut.ExtractPlaceholders("{{alpha}} {{Alpha}} {{beta}} {{alpha}}");

        Assert.Equal(["alpha", "beta"], names);
    }

    [Fact]
    public void Extract_rejects_empty_template()
    {
        var ex = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("   "));
        Assert.Equal(PromptLabErrorCodes.TemplateRequired, ex.Code);
    }

    [Fact]
    public void Extract_rejects_too_long_template()
    {
        var template = new string('a', PromptLabLimits.MaxTemplateLength + 1);
        var ex = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders(template));
        Assert.Equal(PromptLabErrorCodes.TemplateTooLong, ex.Code);
    }

    [Fact]
    public void Extract_rejects_empty_braces()
    {
        var ex = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {{}}"));
        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderInvalid, ex.Code);
    }

    [Fact]
    public void Extract_rejects_whitespace_inside_braces()
    {
        var leading = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {{ name}}"));
        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderInvalid, leading.Code);

        var trailing = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {{name }}"));
        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderInvalid, trailing.Code);

        var both = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {{ name }}"));
        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderInvalid, both.Code);
    }

    [Fact]
    public void Extract_rejects_nested_placeholders()
    {
        var ex = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("{{outer{{inner}}}}"));
        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderInvalid, ex.Code);
    }

    [Fact]
    public void Extract_rejects_malformed_braces()
    {
        var unbalanced = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {{name"));
        Assert.Equal(PromptLabErrorCodes.TemplateSyntaxInvalid, unbalanced.Code);

        var stray = Assert.Throws<DomainException>(() => _sut.ExtractPlaceholders("Hello {name}"));
        Assert.Equal(PromptLabErrorCodes.TemplateSyntaxInvalid, stray.Code);
    }

    [Fact]
    public void Extract_allows_templates_without_placeholders()
    {
        var names = _sut.ExtractPlaceholders("Static prompt with no tokens.");
        Assert.Empty(names);
    }
}
