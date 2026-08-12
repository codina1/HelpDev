/**
 * Minimal, dependency-free Markdown parser for the CMS preview.
 *
 * It produces a typed block/segment tree that the preview renders as React
 * elements (never `dangerouslySetInnerHTML`), so untrusted text cannot inject
 * markup. Supported: headings (1-3), paragraphs, fenced code blocks, ordered /
 * unordered lists, and inline bold / italic / code / links.
 */

export type InlineSegment =
  | { kind: "text"; value: string }
  | { kind: "bold"; value: string }
  | { kind: "italic"; value: string }
  | { kind: "code"; value: string }
  | { kind: "link"; value: string; href: string };

export type MarkdownBlock =
  | { kind: "heading"; level: 1 | 2 | 3; text: string }
  | { kind: "paragraph"; text: string }
  | { kind: "code"; text: string }
  | { kind: "list"; ordered: boolean; items: string[] };

const HEADING = /^(#{1,3})\s+(.*)$/;
const UNORDERED = /^[-*]\s+(.*)$/;
const ORDERED = /^\d+\.\s+(.*)$/;

export function parseMarkdown(input: string): MarkdownBlock[] {
  const lines = input.replace(/\r\n/g, "\n").split("\n");
  const blocks: MarkdownBlock[] = [];

  let paragraph: string[] = [];
  let listItems: string[] = [];
  let listOrdered = false;
  let inList = false;

  const flushParagraph = () => {
    if (paragraph.length > 0) {
      blocks.push({ kind: "paragraph", text: paragraph.join(" ").trim() });
      paragraph = [];
    }
  };
  const flushList = () => {
    if (inList && listItems.length > 0) {
      blocks.push({ kind: "list", ordered: listOrdered, items: listItems });
    }
    listItems = [];
    inList = false;
  };

  for (let i = 0; i < lines.length; i += 1) {
    const line = lines[i];

    // Fenced code block.
    if (line.trim().startsWith("```")) {
      flushParagraph();
      flushList();
      const codeLines: string[] = [];
      i += 1;
      while (i < lines.length && !lines[i].trim().startsWith("```")) {
        codeLines.push(lines[i]);
        i += 1;
      }
      blocks.push({ kind: "code", text: codeLines.join("\n") });
      continue;
    }

    if (line.trim() === "") {
      flushParagraph();
      flushList();
      continue;
    }

    const heading = HEADING.exec(line);
    if (heading) {
      flushParagraph();
      flushList();
      blocks.push({
        kind: "heading",
        level: heading[1].length as 1 | 2 | 3,
        text: heading[2].trim(),
      });
      continue;
    }

    const unordered = UNORDERED.exec(line);
    const ordered = ORDERED.exec(line);
    if (unordered || ordered) {
      flushParagraph();
      const isOrdered = Boolean(ordered);
      if (inList && listOrdered !== isOrdered) flushList();
      inList = true;
      listOrdered = isOrdered;
      listItems.push((unordered?.[1] ?? ordered?.[1] ?? "").trim());
      continue;
    }

    flushList();
    paragraph.push(line.trim());
  }

  flushParagraph();
  flushList();
  return blocks;
}

const INLINE_PATTERNS: Array<{
  kind: InlineSegment["kind"];
  re: RegExp;
}> = [
  { kind: "code", re: /`([^`]+)`/ },
  { kind: "bold", re: /\*\*([^*]+)\*\*/ },
  { kind: "link", re: /\[([^\]]+)\]\(([^)\s]+)\)/ },
  { kind: "italic", re: /\*([^*]+)\*|_([^_]+)_/ },
];

function isSafeHref(href: string): boolean {
  return /^(https?:\/\/|\/|mailto:)/i.test(href);
}

export function parseInline(text: string): InlineSegment[] {
  const segments: InlineSegment[] = [];
  let rest = text;

  while (rest.length > 0) {
    let best: { index: number; length: number; segment: InlineSegment } | null = null;

    for (const pattern of INLINE_PATTERNS) {
      const match = pattern.re.exec(rest);
      if (!match) continue;
      if (best && match.index >= best.index) continue;

      let segment: InlineSegment;
      if (pattern.kind === "link") {
        const href = match[2];
        if (!isSafeHref(href)) continue; // treat unsafe links as plain text
        segment = { kind: "link", value: match[1], href };
      } else if (pattern.kind === "italic") {
        segment = { kind: "italic", value: match[1] ?? match[2] ?? "" };
      } else if (pattern.kind === "bold") {
        segment = { kind: "bold", value: match[1] };
      } else {
        segment = { kind: "code", value: match[1] };
      }

      best = { index: match.index, length: match[0].length, segment };
    }

    if (!best) {
      segments.push({ kind: "text", value: rest });
      break;
    }

    if (best.index > 0) {
      segments.push({ kind: "text", value: rest.slice(0, best.index) });
    }
    segments.push(best.segment);
    rest = rest.slice(best.index + best.length);
  }

  return segments;
}
