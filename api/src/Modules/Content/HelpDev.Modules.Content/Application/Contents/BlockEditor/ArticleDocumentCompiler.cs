using System.Net;
using System.Text;
using System.Text.Json;
using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Application.Contents.BlockEditor;

/// <summary>
/// Compiles TipTap JSON into allowlisted HTML, extracted plain text, and reading stats.
/// Client HTML is never trusted — the server regenerates markup from JSON.
/// </summary>
public static class ArticleDocumentCompiler
{
    public const string EditorVersion = ArticleEditorLimits.CurrentEditorVersion;

    private static readonly HashSet<string> AllowedNodes = new(StringComparer.Ordinal)
    {
        "doc", "paragraph", "heading", "blockquote", "bulletList", "orderedList", "listItem",
        "taskList", "taskItem", "codeBlock", "horizontalRule", "hardBreak", "text",
        "image", "youtube", "table", "tableRow", "tableHeader", "tableCell",
        "callout", "spacer", "gallery", "fileDownload", "terminal", "cta", "articleLink",
    };

    private static readonly HashSet<string> AllowedMarks = new(StringComparer.Ordinal)
    {
        "bold", "italic", "underline", "strike", "code", "link", "highlight", "textStyle",
    };

    public static CompiledArticleDocument Compile(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new DomainException("ساختار بلوکی محتوا الزامی است.");
        }

        var trimmed = contentJson.Trim();
        if (trimmed.Length > ArticleEditorLimits.MaxContentJsonLength)
        {
            throw new DomainException("ساختار بلوکی محتوا بیش از حد مجاز است.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(trimmed);
        }
        catch (JsonException ex)
        {
            throw new DomainException("ساختار بلوکی محتوا نامعتبر است.", ex);
        }

        using (document)
        {
            var root = document.RootElement;
            if (!IsObjectType(root, "doc"))
            {
                throw new DomainException("سند ویرایشگر باید از نوع doc باشد.");
            }

            var html = new StringBuilder();
            var text = new StringBuilder();
            var headings = new List<CompiledHeading>();
            var headingIds = new HashSet<string>(StringComparer.Ordinal);
            RenderNode(root, html, text, headings, headingIds, depth: 0);

            var plain = NormalizePlainText(text.ToString());
            if (string.IsNullOrWhiteSpace(plain))
            {
                throw new DomainException("متن محتوا الزامی است.");
            }

            var htmlValue = html.ToString();
            if (htmlValue.Length > ArticleEditorLimits.MaxContentHtmlLength)
            {
                throw new DomainException("خروجی HTML محتوا بیش از حد مجاز است.");
            }

            var wordCount = CountWords(plain);
            var readingTime = Math.Clamp((int)Math.Ceiling(wordCount / 200d), 1, ArticleEditorLimits.MaxReadingTimeMinutes);
            var canonicalJson = Canonicalize(trimmed);
            return new CompiledArticleDocument(canonicalJson, htmlValue, plain, wordCount, readingTime, headings);
        }
    }

    public static CompiledArticleDocument? TryCompile(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            return null;
        }

        try
        {
            return Compile(contentJson);
        }
        catch (DomainException)
        {
            return null;
        }
    }

    private static string Canonicalize(string json)
    {
        using var parsed = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(parsed.RootElement, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    private static void RenderNode(
        JsonElement node,
        StringBuilder html,
        StringBuilder text,
        List<CompiledHeading> headings,
        HashSet<string> headingIds,
        int depth)
    {
        if (depth > 40)
        {
            throw new DomainException("ساختار بلوکی محتوا بیش از حد تو در تو است.");
        }

        if (node.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var type = GetString(node, "type");
        if (string.IsNullOrEmpty(type) || !AllowedNodes.Contains(type))
        {
            RenderChildren(node, html, text, headings, headingIds, depth);
            return;
        }

        switch (type)
        {
            case "doc":
                RenderChildren(node, html, text, headings, headingIds, depth);
                break;
            case "paragraph":
                html.Append(OpenAlignedTag("p", GetAttrString(node, "textAlign")));
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</p>");
                text.AppendLine();
                break;
            case "heading":
                var level = ClampInt(GetAttrInt(node, "level"), 2, 4);
                var innerHtml = new StringBuilder();
                var headingText = new StringBuilder();
                RenderChildren(node, innerHtml, headingText, headings, headingIds, depth);
                var headingPlain = NormalizePlainText(headingText.ToString());
                var headingId = MakeHeadingId(headingPlain, headingIds);
                html.Append(OpenAlignedTag("h" + level, GetAttrString(node, "textAlign"), headingId));
                html.Append(innerHtml);
                html.Append("</h").Append(level).Append('>');
                if (!string.IsNullOrWhiteSpace(headingPlain))
                {
                    headings.Add(new CompiledHeading(headingId, level, headingPlain));
                    text.AppendLine(headingPlain);
                }
                break;
            case "blockquote":
                html.Append("<blockquote>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</blockquote>");
                text.AppendLine();
                break;
            case "bulletList":
                html.Append("<ul>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</ul>");
                break;
            case "orderedList":
                html.Append("<ol>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</ol>");
                break;
            case "taskList":
                html.Append("<ul class=\"hd-task-list\">");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</ul>");
                break;
            case "listItem":
                html.Append("<li>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</li>");
                text.AppendLine();
                break;
            case "taskItem":
                var checkedItem = GetAttrBool(node, "checked");
                html.Append("<li class=\"hd-task-item\"><input type=\"checkbox\" disabled");
                if (checkedItem) html.Append(" checked");
                html.Append(" /><span>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</span></li>");
                text.AppendLine();
                break;
            case "codeBlock":
                var language = SanitizeToken(GetAttrString(node, "language"), 40);
                var showLines = GetAttrBool(node, "showLineNumbers");
                html.Append("<pre class=\"hd-code-block\"");
                if (!string.IsNullOrEmpty(language))
                {
                    html.Append(" data-language=\"").Append(Enc(language)).Append('"');
                }
                if (showLines) html.Append(" data-line-numbers=\"true\"");
                html.Append("><code>");
                RenderPlainTextChildren(node, html, text);
                html.Append("</code></pre>");
                text.AppendLine();
                break;
            case "terminal":
                html.Append("<pre class=\"hd-terminal\"><code>");
                RenderPlainTextChildren(node, html, text);
                html.Append("</code></pre>");
                text.AppendLine();
                break;
            case "horizontalRule":
                html.Append("<hr />");
                break;
            case "hardBreak":
                html.Append("<br />");
                text.AppendLine();
                break;
            case "spacer":
                var height = ClampInt(GetAttrInt(node, "height"), 8, 240);
                html.Append("<div class=\"hd-spacer\" style=\"height:").Append(height).Append("px\" aria-hidden=\"true\"></div>");
                break;
            case "callout":
                var variant = SanitizeToken(GetAttrString(node, "variant"), 20);
                if (variant is not ("info" or "warning" or "success" or "note" or "tip"))
                {
                    variant = "info";
                }
                html.Append("<aside class=\"hd-callout hd-callout-").Append(Enc(variant)).Append("\">");
                var calloutTitle = GetAttrString(node, "title");
                if (!string.IsNullOrWhiteSpace(calloutTitle))
                {
                    html.Append("<p class=\"hd-callout-title\"><strong>").Append(Enc(calloutTitle.Trim())).Append("</strong></p>");
                    text.AppendLine(calloutTitle.Trim());
                }
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</aside>");
                text.AppendLine();
                break;
            case "image":
                RenderImage(node, html, text);
                break;
            case "gallery":
                html.Append("<div class=\"hd-gallery\">");
                RenderGallery(node, html, text);
                html.Append("</div>");
                break;
            case "youtube":
                RenderYoutube(node, html);
                break;
            case "fileDownload":
                RenderFile(node, html, text);
                break;
            case "cta":
                RenderCta(node, html, text);
                break;
            case "articleLink":
                RenderArticleLink(node, html, text);
                break;
            case "table":
                html.Append("<div class=\"hd-table-wrap\"><table>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</table></div>");
                break;
            case "tableRow":
                html.Append("<tr>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</tr>");
                break;
            case "tableHeader":
                html.Append("<th>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</th>");
                break;
            case "tableCell":
                html.Append("<td>");
                RenderChildren(node, html, text, headings, headingIds, depth);
                html.Append("</td>");
                break;
            case "text":
                RenderText(node, html, text);
                break;
            default:
                RenderChildren(node, html, text, headings, headingIds, depth);
                break;
        }
    }

    private static void RenderChildren(
        JsonElement node,
        StringBuilder html,
        StringBuilder text,
        List<CompiledHeading> headings,
        HashSet<string> headingIds,
        int depth)
    {
        if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var child in content.EnumerateArray())
        {
            RenderNode(child, html, text, headings, headingIds, depth + 1);
        }
    }

    private static void RenderPlainTextChildren(JsonElement node, StringBuilder html, StringBuilder text)
    {
        if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var child in content.EnumerateArray())
        {
            var value = GetString(child, "text");
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            html.Append(Enc(value));
            text.Append(value);
        }
    }

    private static void RenderText(JsonElement node, StringBuilder html, StringBuilder text)
    {
        var value = GetString(node, "text");
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        text.Append(value);
        var encoded = Enc(value);
        if (!node.TryGetProperty("marks", out var marks) || marks.ValueKind != JsonValueKind.Array)
        {
            html.Append(encoded);
            return;
        }

        var open = new List<string>();
        var close = new List<string>();
        foreach (var mark in marks.EnumerateArray())
        {
            var markType = GetString(mark, "type");
            if (string.IsNullOrEmpty(markType) || !AllowedMarks.Contains(markType))
            {
                continue;
            }

            switch (markType)
            {
                case "bold":
                    open.Add("<strong>");
                    close.Add("</strong>");
                    break;
                case "italic":
                    open.Add("<em>");
                    close.Add("</em>");
                    break;
                case "underline":
                    open.Add("<u>");
                    close.Add("</u>");
                    break;
                case "strike":
                    open.Add("<s>");
                    close.Add("</s>");
                    break;
                case "code":
                    open.Add("<code>");
                    close.Add("</code>");
                    break;
                case "highlight":
                    open.Add("<mark>");
                    close.Add("</mark>");
                    break;
                case "textStyle":
                    var color = SanitizeCssColor(GetAttrString(mark, "color"));
                    if (color is null)
                    {
                        break;
                    }

                    open.Add($"<span style=\"color:{color}\">");
                    close.Add("</span>");
                    break;
                case "link":
                    var href = SanitizeUrl(GetAttrString(mark, "href"));
                    if (href is null)
                    {
                        break;
                    }

                    var target = GetAttrString(mark, "target") == "_blank" ? "_blank" : null;
                    var relParts = new List<string>();
                    if (target == "_blank")
                    {
                        relParts.Add("noopener");
                        relParts.Add("noreferrer");
                    }
                    var rel = GetAttrString(mark, "rel");
                    if (ContainsToken(rel, "nofollow")) relParts.Add("nofollow");
                    if (ContainsToken(rel, "sponsored")) relParts.Add("sponsored");
                    open.Add($"<a href=\"{Enc(href)}\""
                        + (target is null ? string.Empty : " target=\"_blank\"")
                        + (relParts.Count == 0 ? string.Empty : $" rel=\"{Enc(string.Join(' ', relParts))}\"")
                        + ">");
                    close.Add("</a>");
                    break;
            }
        }

        foreach (var tag in open) html.Append(tag);
        html.Append(encoded);
        for (var i = close.Count - 1; i >= 0; i--) html.Append(close[i]);
    }

    private static void RenderImage(JsonElement node, StringBuilder html, StringBuilder text)
    {
        var src = SanitizeUrl(GetAttrString(node, "src"), image: true);
        if (src is null)
        {
            return;
        }

        var alt = GetAttrString(node, "alt") ?? string.Empty;
        var title = GetAttrString(node, "title");
        var caption = GetAttrString(node, "caption");
        var align = SanitizeToken(GetAttrString(node, "align"), 16);
        if (align is not ("right" or "left" or "center" or "wide" or "full"))
        {
            align = "center";
        }

        var width = ClampInt(GetAttrInt(node, "width"), 0, 2400);
        var height = ClampInt(GetAttrInt(node, "height"), 0, 2400);
        var href = SanitizeUrl(GetAttrString(node, "href"));
        html.Append("<figure class=\"hd-image hd-image-").Append(Enc(align)).Append("\">");
        if (href is not null)
        {
            var target = GetAttrString(node, "target") == "_blank" ? " target=\"_blank\" rel=\"noopener noreferrer\"" : string.Empty;
            html.Append("<a href=\"").Append(Enc(href)).Append('"').Append(target).Append('>');
        }

        html.Append("<img src=\"").Append(Enc(src)).Append("\" alt=\"").Append(Enc(alt)).Append('"');
        if (!string.IsNullOrEmpty(title)) html.Append(" title=\"").Append(Enc(title)).Append('"');
        if (width > 0) html.Append(" width=\"").Append(width).Append('"');
        if (height > 0) html.Append(" height=\"").Append(height).Append('"');
        html.Append(" loading=\"lazy\" />");
        if (href is not null) html.Append("</a>");
        if (!string.IsNullOrWhiteSpace(caption))
        {
            html.Append("<figcaption>").Append(Enc(caption.Trim())).Append("</figcaption>");
            text.AppendLine(caption.Trim());
        }

        html.Append("</figure>");
    }

    private static void RenderGallery(JsonElement node, StringBuilder html, StringBuilder text)
    {
        if (!TryGetAttrs(node, out var attrs) || !attrs.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var src = SanitizeUrl(GetString(item, "src"), image: true);
            if (src is null)
            {
                continue;
            }

            var alt = GetString(item, "alt") ?? string.Empty;
            html.Append("<figure class=\"hd-gallery-item\"><img src=\"")
                .Append(Enc(src))
                .Append("\" alt=\"")
                .Append(Enc(alt))
                .Append("\" loading=\"lazy\" /></figure>");
        }
    }

    private static void RenderYoutube(JsonElement node, StringBuilder html)
    {
        var src = SanitizeYoutube(GetAttrString(node, "src") ?? GetAttrString(node, "url"));
        if (src is null)
        {
            return;
        }

        html.Append("<div class=\"hd-embed\"><iframe src=\"")
            .Append(Enc(src))
            .Append("\" title=\"YouTube\" loading=\"lazy\" allowfullscreen referrerpolicy=\"strict-origin-when-cross-origin\"></iframe></div>");
    }

    private static void RenderFile(JsonElement node, StringBuilder html, StringBuilder text)
    {
        var href = SanitizeUrl(GetAttrString(node, "href"));
        var name = GetAttrString(node, "name") ?? "دانلود فایل";
        if (href is null)
        {
            return;
        }

        html.Append("<p class=\"hd-file\"><a href=\"").Append(Enc(href)).Append("\" download>")
            .Append(Enc(name)).Append("</a></p>");
        text.AppendLine(name);
    }

    private static void RenderCta(JsonElement node, StringBuilder html, StringBuilder text)
    {
        var href = SanitizeUrl(GetAttrString(node, "href"));
        var label = GetAttrString(node, "label") ?? "ادامه مطلب";
        if (href is null)
        {
            return;
        }

        html.Append("<p class=\"hd-cta\"><a class=\"hd-cta-button\" href=\"").Append(Enc(href)).Append("\">")
            .Append(Enc(label)).Append("</a></p>");
        text.AppendLine(label);
    }

    private static void RenderArticleLink(JsonElement node, StringBuilder html, StringBuilder text)
    {
        var href = SanitizeUrl(GetAttrString(node, "href"));
        var title = GetAttrString(node, "title") ?? GetAttrString(node, "slug") ?? "مقاله مرتبط";
        if (href is null)
        {
            return;
        }

        html.Append("<p class=\"hd-article-link\"><a href=\"").Append(Enc(href)).Append("\">")
            .Append(Enc(title)).Append("</a></p>");
        text.AppendLine(title);
    }

    private static bool IsObjectType(JsonElement element, string type) =>
        element.ValueKind == JsonValueKind.Object && GetString(element, "type") == type;

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool TryGetAttrs(JsonElement node, out JsonElement attrs)
    {
        if (node.TryGetProperty("attrs", out attrs) && attrs.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        attrs = default;
        return false;
    }

    private static string? GetAttrString(JsonElement node, string name)
    {
        if (TryGetAttrs(node, out var attrs) && attrs.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static int GetAttrInt(JsonElement node, string name)
    {
        if (!TryGetAttrs(node, out var attrs) || !attrs.TryGetProperty(name, out var value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static bool GetAttrBool(JsonElement node, string name)
    {
        if (!TryGetAttrs(node, out var attrs) || !attrs.TryGetProperty(name, out var value))
        {
            return false;
        }

        return value.ValueKind == JsonValueKind.True
            || (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed) && parsed);
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string OpenAlignedTag(string tag, string? align, string? id = null)
    {
        var safeAlign = align is "left" or "right" or "center" or "justify" ? align : null;
        var builder = new StringBuilder("<").Append(tag);
        if (!string.IsNullOrEmpty(id))
        {
            builder.Append(" id=\"").Append(Enc(id)).Append('"');
        }

        if (safeAlign is not null)
        {
            builder.Append(" class=\"hd-align-").Append(safeAlign).Append('"');
        }

        return builder.Append('>').ToString();
    }

    private static string MakeHeadingId(string text, HashSet<string> used)
    {
        var builder = new StringBuilder();
        foreach (var ch in text.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var baseId = builder.ToString().Trim('-');
        if (baseId.Length > 80)
        {
            baseId = baseId[..80].Trim('-');
        }

        if (string.IsNullOrEmpty(baseId))
        {
            baseId = "h";
        }

        var id = baseId;
        var n = 2;
        while (!used.Add(id))
        {
            id = $"{baseId}-{n++}";
        }

        return id;
    }

    private static string? SanitizeCssColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length is 4 or 7
            && trimmed[0] == '#'
            && trimmed.Skip(1).All(ch => Uri.IsHexDigit(ch)))
        {
            return trimmed.ToLowerInvariant();
        }

        return null;
    }

    private static int ClampInt(int value, int min, int max) => Math.Clamp(value, min, max);

    private static string SanitizeToken(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var ch in value.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_')
            {
                builder.Append(ch);
            }
        }

        return builder.Length > maxLength ? builder.ToString(0, maxLength) : builder.ToString();
    }

    private static bool ContainsToken(string? value, string token) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(token, StringComparer.OrdinalIgnoreCase);

    private static string? SanitizeUrl(string? value, bool image = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 2048 || trimmed.Contains('\\', StringComparison.Ordinal) || trimmed.Contains('\0'))
        {
            return null;
        }

        if (trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return null;
        }

        if (trimmed.StartsWith("/", StringComparison.Ordinal) && !trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || (!image && trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)))
        {
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.Scheme is not ("http" or "https" or "mailto"))
            {
                return null;
            }

            return uri.ToString();
        }

        return null;
    }

    private static string? SanitizeYoutube(string? value)
    {
        var url = SanitizeUrl(value);
        if (url is null || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host.ToLowerInvariant();
        if (host is "www.youtube.com" or "youtube.com" or "www.youtube-nocookie.com" or "youtube-nocookie.com")
        {
            if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
            {
                return $"https://www.youtube-nocookie.com{uri.AbsolutePath}";
            }

            var id = GetQueryValue(uri.Query, "v");
            if (!string.IsNullOrWhiteSpace(id) && id.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'))
            {
                return $"https://www.youtube-nocookie.com/embed/{id}";
            }
        }

        if (host is "youtu.be")
        {
            var id = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(id) && id.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'))
            {
                return $"https://www.youtube-nocookie.com/embed/{id}";
            }
        }

        return null;
    }

    private static string? GetQueryValue(string query, string name)
    {
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        var trimmed = query.TrimStart('?');
        foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split('=', 2);
            if (pieces.Length == 2 && string.Equals(pieces[0], name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(pieces[1]);
            }
        }

        return null;
    }

    private static string NormalizePlainText(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static int CountWords(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

public sealed record CompiledArticleDocument(
    string ContentJson,
    string ContentHtml,
    string PlainText,
    int WordCount,
    int ReadingTimeMinutes,
    IReadOnlyList<CompiledHeading> Headings);

public sealed record CompiledHeading(string Id, int Level, string Text);
