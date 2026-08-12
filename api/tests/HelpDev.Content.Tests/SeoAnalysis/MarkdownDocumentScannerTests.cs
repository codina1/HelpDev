using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Content.Tests.SeoAnalysis;

public sealed class MarkdownDocumentScannerTests
{
    [Fact]
    public void Scan_parses_headings_paragraphs_links_and_code()
    {
        var body = """
            # Title H1

            First paragraph with [internal](/path) and [ext](https://example.com).

            ## Section

            ```csharp
            Console.WriteLine(1);
            ```

            ```
            unlabelled
            ```
            """;

        var facts = MarkdownDocumentScanner.Scan(body);

        Assert.Equal(2, facts.Headings.Count);
        Assert.Equal(1, facts.Headings[0].Level);
        Assert.Equal("Title H1", facts.Headings[0].Text);
        Assert.Single(facts.Paragraphs);
        Assert.Contains("First paragraph", facts.Paragraphs[0].Text);
        Assert.Equal(2, facts.Links.Count);
        Assert.Equal(2, facts.CodeBlocks.Count);
        Assert.Equal(1, facts.LanguageLabelledCodeBlockCount);
        Assert.Equal(1, facts.UnlabelledCodeBlockCount);
        Assert.True(facts.WordCount > 0);
        Assert.True(facts.CharacterCount > 0);
    }

    [Fact]
    public void Scan_skips_code_blocks_as_paragraphs()
    {
        var facts = MarkdownDocumentScanner.Scan("""
            Intro text here.

            ```js
            const x = 1;
            ```

            After code.
            """);

        Assert.Equal(2, facts.Paragraphs.Count);
        Assert.DoesNotContain(facts.Paragraphs, p => p.Text.Contains("const x", StringComparison.Ordinal));
        Assert.Single(facts.CodeBlocks);
    }

    [Fact]
    public void CountWords_supports_persian_and_english()
    {
        Assert.Equal(3, MarkdownDocumentScanner.CountWords("hello world test"));
        Assert.True(MarkdownDocumentScanner.CountWords("سلام دنیای برنامه") >= 2);
        Assert.Equal(0, MarkdownDocumentScanner.CountWords("   "));
    }

    [Fact]
    public void Keyword_matching_is_case_insensitive_and_avoids_substring_false_positives()
    {
        Assert.Equal(1, MarkdownDocumentScanner.CountKeywordOccurrences("Learn ASP.NET today", "asp.net"));
        Assert.Equal(0, MarkdownDocumentScanner.CountKeywordOccurrences("javascripting", "script"));
        Assert.True(MarkdownDocumentScanner.ContainsKeyword("کلیدواژه اصلی", "کلیدواژه"));
    }

    [Fact]
    public void Scan_large_body_is_bounded()
    {
        var paragraph = string.Join(' ', Enumerable.Repeat("word", 50));
        var body = string.Join("\n\n", Enumerable.Range(0, 200).Select(i =>
            i % 10 == 0 ? $"## Heading {i}\n\n{paragraph}" : paragraph));

        var facts = MarkdownDocumentScanner.Scan(body);

        Assert.True(facts.WordCount > 1000);
        Assert.True(facts.Headings.Count >= 20);
        Assert.True(facts.Paragraphs.Count > 100);
    }
}
