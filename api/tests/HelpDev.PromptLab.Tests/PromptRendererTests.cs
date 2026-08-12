using System.Text.Json;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptRendererTests
{
    private readonly PromptRenderer _sut = new();

    [Fact]
    public void Render_replaces_placeholders()
    {
        var output = _sut.Render(
            CreateSnapshot("Hello {{name}}", CreateText("name", required: true)),
            Values(("name", "\"Ada\"")));

        Assert.Equal("Hello Ada", output.RenderedText);
    }

    [Fact]
    public void Render_replaces_repeated_placeholder()
    {
        var output = _sut.Render(
            CreateSnapshot("{{x}} and {{x}}", CreateText("x", required: true)),
            Values(("x", "\"A\"")));

        Assert.Equal("A and A", output.RenderedText);
    }

    [Fact]
    public void Render_missing_required_throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _sut.Render(
                CreateSnapshot("Hello {{name}}", CreateText("name", required: true)),
                new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)));

        Assert.Equal(PromptLabErrorCodes.RenderRequiredVariableMissing, ex.Code);
    }

    [Fact]
    public void Render_uses_default_when_missing()
    {
        var output = _sut.Render(
            CreateSnapshot(
                "Hello {{name}}",
                new PromptVariableSnapshot(
                    "name",
                    PromptVariableType.Text,
                    IsRequired: false,
                    DefaultValue: "World",
                    MinLength: null,
                    MaxLength: null,
                    MinValue: null,
                    MaxValue: null,
                    ValidationPattern: null,
                    AllowedValues: [])),
            new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase));

        Assert.Equal("Hello World", output.RenderedText);
    }

    [Fact]
    public void Render_unknown_supplied_throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _sut.Render(
                CreateSnapshot("Hello {{name}}", CreateText("name", required: true)),
                Values(("name", "\"Ada\""), ("extra", "\"nope\""))));

        Assert.Equal(PromptLabErrorCodes.RenderUnknownVariable, ex.Code);
    }

    [Fact]
    public void Render_rejects_invalid_types()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _sut.Render(
                CreateSnapshot(
                    "{{count}}",
                    new PromptVariableSnapshot(
                        "count",
                        PromptVariableType.Integer,
                        true,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        [])),
                Values(("count", "\"abc\""))));

        Assert.Equal(PromptLabErrorCodes.RenderValueInvalid, ex.Code);
    }

    [Fact]
    public void Render_select_rejects_unknown_option()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _sut.Render(
                CreateSnapshot(
                    "{{lang}}",
                    new PromptVariableSnapshot(
                        "lang",
                        PromptVariableType.Select,
                        true,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        ["cs", "ts"])),
                Values(("lang", "\"java\""))));

        Assert.Equal(PromptLabErrorCodes.RenderValueInvalid, ex.Code);
    }

    [Fact]
    public void Render_pattern_timeout_or_invalid_for_catastrophic_regex()
    {
        // Match must fail to trigger catastrophic backtracking: (a+)+b vs aaaa...
        var longValue = new string('a', PromptLabLimits.MaxVariableValueLength);
        var snapshot = CreateSnapshot(
            "{{payload}}",
            new PromptVariableSnapshot(
                "payload",
                PromptVariableType.Text,
                true,
                null,
                null,
                PromptLabLimits.MaxVariableValueLength,
                null,
                null,
                "(a+)+b",
                []));

        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["payload"] = JsonSerializer.SerializeToElement(longValue),
        };

        var ex = Assert.Throws<DomainException>(() => _sut.Render(snapshot, values));
        Assert.True(
            ex.Code is PromptLabErrorCodes.RenderPatternTimeout or PromptLabErrorCodes.RenderValueInvalid,
            $"Unexpected code: {ex.Code}");
    }

    [Fact]
    public void Render_keeps_braces_in_user_value_literal_without_recursive_render()
    {
        var output = _sut.Render(
            CreateSnapshot("Template {{x}}", CreateText("x", required: true)),
            Values(("x", "\"{{y}}\"")));

        Assert.Equal("Template {{y}}", output.RenderedText);
        Assert.Contains("{{y}}", output.RenderedText, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_rejects_output_too_long()
    {
        var chunk = new string('x', PromptLabLimits.MaxVariableValueLength);
        var snapshot = CreateSnapshot(
            "{{a}}{{a}}{{a}}",
            new PromptVariableSnapshot(
                "a",
                PromptVariableType.Text,
                true,
                null,
                null,
                PromptLabLimits.MaxVariableValueLength,
                null,
                null,
                null,
                []));

        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = JsonSerializer.SerializeToElement(chunk),
        };

        var ex = Assert.Throws<DomainException>(() => _sut.Render(snapshot, values));
        Assert.Equal(PromptLabErrorCodes.RenderOutputTooLong, ex.Code);
    }

    private static PromptVersionSnapshot CreateSnapshot(
        string template,
        params PromptVariableSnapshot[] variables) =>
        new(Guid.NewGuid(), 1, template, variables);

    private static PromptVariableSnapshot CreateText(string name, bool required) =>
        new(name, PromptVariableType.Text, required, null, null, null, null, null, null, []);

    private static Dictionary<string, JsonElement> Values(params (string Key, string Json)[] pairs)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, json) in pairs)
        {
            using var document = JsonDocument.Parse(json);
            values[key] = document.RootElement.Clone();
        }

        return values;
    }
}
