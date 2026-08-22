using HelpDev.Modules.Content.Application.Contents.BlockEditor;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests;

public sealed class ArticleDocumentCompilerTests
{
    [Fact]
    public void Compile_renders_paragraph_heading_and_image_between_blocks()
    {
        var json = """
        {
          "type": "doc",
          "content": [
            { "type": "paragraph", "content": [{ "type": "text", "text": "سلام دنیا" }] },
            { "type": "image", "attrs": { "src": "/media/2026/08/cover.png", "alt": "کاور", "align": "center" } },
            { "type": "heading", "attrs": { "level": 2 }, "content": [{ "type": "text", "text": "تیتر دوم" }] }
          ]
        }
        """;

        var compiled = ArticleDocumentCompiler.Compile(json);

        Assert.Contains("<p>سلام دنیا</p>", compiled.ContentHtml);
        Assert.Contains("src=\"/media/2026/08/cover.png\"", compiled.ContentHtml);
        Assert.Contains("alt=\"کاور\"", compiled.ContentHtml);
        Assert.Contains("loading=\"lazy\"", compiled.ContentHtml);
        Assert.Contains("<h2", compiled.ContentHtml);
        Assert.Contains("تیتر دوم", compiled.ContentHtml);
        Assert.Single(compiled.Headings);
        Assert.True(compiled.WordCount >= 3);
    }

    [Fact]
    public void Compile_strips_javascript_urls_and_unknown_html()
    {
        var json = """
        {
          "type": "doc",
          "content": [
            {
              "type": "paragraph",
              "content": [
                {
                  "type": "text",
                  "text": "click",
                  "marks": [{ "type": "link", "attrs": { "href": "javascript:alert(1)" } }]
                }
              ]
            },
            { "type": "paragraph", "content": [{ "type": "text", "text": "<script>alert(1)</script>" }] }
          ]
        }
        """;

        var compiled = ArticleDocumentCompiler.Compile(json);

        Assert.DoesNotContain("javascript:", compiled.ContentHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script>", compiled.ContentHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", compiled.ContentHtml);
    }

    [Fact]
    public void Compile_rejects_empty_document()
    {
        var json = """{ "type": "doc", "content": [{ "type": "paragraph" }] }""";
        Assert.Throws<DomainException>(() => ArticleDocumentCompiler.Compile(json));
    }
}
