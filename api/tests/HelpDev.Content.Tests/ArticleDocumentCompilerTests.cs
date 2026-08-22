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
        Assert.Contains("id=\"تیتر-دوم\"", compiled.ContentHtml);
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
    public void Compile_strips_base64_images_and_unknown_marks()
    {
        var json = """
        {
          "type": "doc",
          "content": [
            { "type": "image", "attrs": { "src": "data:image/png;base64,abc" } },
            {
              "type": "paragraph",
              "content": [
                {
                  "type": "text",
                  "text": "رنگی",
                  "marks": [
                    { "type": "highlight" },
                    { "type": "textStyle", "attrs": { "color": "#e11d48" } },
                    { "type": "textStyle", "attrs": { "color": "expression(alert(1))" } }
                  ]
                }
              ]
            }
          ]
        }
        """;

        var compiled = ArticleDocumentCompiler.Compile(json);

        Assert.DoesNotContain("data:image", compiled.ContentHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<mark>", compiled.ContentHtml);
        Assert.Contains("color:#e11d48", compiled.ContentHtml);
        Assert.DoesNotContain("expression(", compiled.ContentHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compile_renders_callout_table_and_safe_blank_links()
    {
        var json = """
        {
          "type": "doc",
          "content": [
            {
              "type": "callout",
              "attrs": { "variant": "warning", "title": "توجه" },
              "content": [{ "type": "paragraph", "content": [{ "type": "text", "text": "هشدار مهم" }] }]
            },
            {
              "type": "paragraph",
              "content": [{
                "type": "text",
                "text": "لینک",
                "marks": [{ "type": "link", "attrs": { "href": "https://helpdev.ir", "target": "_blank" } }]
              }]
            },
            {
              "type": "table",
              "content": [{
                "type": "tableRow",
                "content": [{ "type": "tableHeader", "content": [{ "type": "paragraph", "content": [{ "type": "text", "text": "ستون" }] }] }]
              }]
            }
          ]
        }
        """;

        var compiled = ArticleDocumentCompiler.Compile(json);

        Assert.Contains("hd-callout-warning", compiled.ContentHtml);
        Assert.Contains("hd-callout-title", compiled.ContentHtml);
        Assert.Contains("noopener noreferrer", compiled.ContentHtml);
        Assert.Contains("hd-table-wrap", compiled.ContentHtml);
        Assert.DoesNotContain("onclick", compiled.ContentHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compile_rejects_empty_document()
    {
        var json = """{ "type": "doc", "content": [{ "type": "paragraph" }] }""";
        Assert.Throws<DomainException>(() => ArticleDocumentCompiler.Compile(json));
    }
}
