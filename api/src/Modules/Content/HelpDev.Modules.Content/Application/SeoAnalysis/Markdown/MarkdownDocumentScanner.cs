using System.Globalization;
using System.Text.RegularExpressions;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

/// <summary>
/// Bounded, dependency-free Markdown scanner for SEO analysis.
/// Supports: ATX headings (1–6), paragraphs, fenced code blocks, Markdown links.
/// Does not execute HTML or fetch URLs.
/// </summary>
public static class MarkdownDocumentScanner
{
    private static readonly Regex HeadingPattern = new(
        @"^(#{1,6})\s+(.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LinkPattern = new(
        @"\[([^\]]*)\]\(([^)\s]+)(?:\s+""[^""]*"")?\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImagePattern = new(
        @"!\[([^\]]*)\]\(([^)\s]+)(?:\s+""[^""]*"")?\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static MarkdownDocumentFacts Scan(string? body)
    {
        var normalized = (body ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n');

        var headings = new List<MarkdownHeading>();
        var paragraphs = new List<MarkdownParagraph>();
        var codeBlocks = new List<MarkdownCodeBlock>();
        var links = new List<MarkdownLink>();
        var images = new List<MarkdownImage>();

        var paragraphBuffer = new List<string>();
        var inCode = false;
        var codeLanguage = (string?)null;
        var codeLines = new List<string>();
        var codeStartLine = 0;

        void FlushParagraph(int lineIndex)
        {
            if (paragraphBuffer.Count == 0)
            {
                return;
            }

            var text = string.Join(' ', paragraphBuffer).Trim();
            paragraphBuffer.Clear();
            if (text.Length == 0)
            {
                return;
            }

            paragraphs.Add(new MarkdownParagraph(text, lineIndex));
            CollectLinks(text, lineIndex, links);
            CollectImages(text, lineIndex, images);
        }

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            if (inCode)
            {
                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    codeBlocks.Add(new MarkdownCodeBlock(
                        string.Join('\n', codeLines),
                        codeLanguage,
                        codeStartLine));
                    inCode = false;
                    codeLanguage = null;
                    codeLines.Clear();
                }
                else
                {
                    codeLines.Add(line);
                }

                continue;
            }

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                FlushParagraph(i);
                inCode = true;
                codeStartLine = i;
                codeLanguage = trimmed.Length > 3
                    ? trimmed[3..].Trim()
                    : null;
                if (string.IsNullOrWhiteSpace(codeLanguage))
                {
                    codeLanguage = null;
                }

                codeLines.Clear();
                continue;
            }

            if (trimmed.Length == 0)
            {
                FlushParagraph(i);
                continue;
            }

            var headingMatch = HeadingPattern.Match(line);
            if (headingMatch.Success)
            {
                FlushParagraph(i);
                var level = headingMatch.Groups[1].Value.Length;
                var text = headingMatch.Groups[2].Value.Trim();
                headings.Add(new MarkdownHeading(level, text, i));
                CollectLinks(text, i, links);
                CollectImages(text, i, images);
                continue;
            }

            // Skip list markers as standalone structure; still count as paragraph text.
            paragraphBuffer.Add(trimmed);
        }

        FlushParagraph(lines.Length);

        if (inCode)
        {
            // Unclosed fence — still count the block for analysis.
            codeBlocks.Add(new MarkdownCodeBlock(
                string.Join('\n', codeLines),
                codeLanguage,
                codeStartLine));
        }

        var wordCount = CountWords(normalized);
        return new MarkdownDocumentFacts(
            CharacterCount: normalized.Length,
            WordCount: wordCount,
            Headings: headings,
            Paragraphs: paragraphs,
            CodeBlocks: codeBlocks,
            Links: links,
            Images: images);
    }

    private static void CollectImages(string text, int lineIndex, List<MarkdownImage> images)
    {
        foreach (Match match in ImagePattern.Matches(text))
        {
            images.Add(new MarkdownImage(
                match.Groups[1].Value,
                match.Groups[2].Value.Trim(),
                lineIndex));
        }
    }

    private static void CollectLinks(string text, int lineIndex, List<MarkdownLink> links)
    {
        foreach (Match match in LinkPattern.Matches(text))
        {
            links.Add(new MarkdownLink(
                match.Groups[1].Value,
                match.Groups[2].Value.Trim(),
                lineIndex));
        }
    }

    /// <summary>
    /// Word count for Persian/English: sequences of letters/digits/marks.
    /// </summary>
    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var count = 0;
        var inWord = false;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch)
                || char.GetUnicodeCategory(ch) is UnicodeCategory.NonSpacingMark
                    or UnicodeCategory.SpacingCombiningMark)
            {
                if (!inWord)
                {
                    count++;
                    inWord = true;
                }
            }
            else
            {
                inWord = false;
            }
        }

        return count;
    }

    /// <summary>
    /// Case-insensitive keyword occurrence using culture-invariant comparison.
    /// Avoids trivial substring matches by requiring word-ish boundaries where practical.
    /// </summary>
    public static int CountKeywordOccurrences(string? haystack, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(haystack) || string.IsNullOrWhiteSpace(keyword))
        {
            return 0;
        }

        var source = haystack.Trim();
        var needle = keyword.Trim();
        if (needle.Length == 0)
        {
            return 0;
        }

        var count = 0;
        var index = 0;
        while (index <= source.Length - needle.Length)
        {
            var found = source.IndexOf(needle, index, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
            {
                break;
            }

            var beforeOk = found == 0 || !IsWordChar(source[found - 1]);
            var afterIndex = found + needle.Length;
            var afterOk = afterIndex >= source.Length || !IsWordChar(source[afterIndex]);
            if (beforeOk && afterOk)
            {
                count++;
            }

            index = found + Math.Max(1, needle.Length);
        }

        return count;
    }

    public static bool ContainsKeyword(string? haystack, string? keyword) =>
        CountKeywordOccurrences(haystack, keyword) > 0;

    private static bool IsWordChar(char ch) =>
        char.IsLetterOrDigit(ch)
        || char.GetUnicodeCategory(ch) is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark;
}
