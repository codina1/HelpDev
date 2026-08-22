import type { JSONContent } from "@tiptap/core";
import { parseInline, parseMarkdown, type InlineSegment } from "@/lib/admin/content/markdown";
import { EMPTY_ARTICLE_DOC } from "@/lib/admin/content/block-editor/document";

/**
 * Converts a legacy Markdown/plain `body` into TipTap JSON.
 * Unsafe `javascript:` links are dropped by `parseInline`.
 */
export function markdownToTiptapDoc(markdown: string): JSONContent {
  const blocks = parseMarkdown(markdown ?? "");
  if (blocks.length === 0) return EMPTY_ARTICLE_DOC;

  const content: JSONContent[] = blocks.map((block) => {
    if (block.kind === "heading") {
      const level = block.level === 1 ? 2 : Math.min(block.level, 3);
      return {
        type: "heading",
        attrs: { level },
        content: inlineToNodes(block.text),
      };
    }
    if (block.kind === "code") {
      return {
        type: "codeBlock",
        attrs: { language: null, showLineNumbers: false },
        content: block.text ? [{ type: "text", text: block.text }] : [],
      };
    }
    if (block.kind === "list") {
      return {
        type: block.ordered ? "orderedList" : "bulletList",
        content: block.items.map((item) => ({
          type: "listItem",
          content: [
            {
              type: "paragraph",
              content: inlineToNodes(item),
            },
          ],
        })),
      };
    }
    return {
      type: "paragraph",
      content: inlineToNodes(block.text),
    };
  });

  return { type: "doc", content };
}

function inlineToNodes(text: string): JSONContent[] {
  const segments = parseInline(text);
  const nodes: JSONContent[] = [];
  for (const segment of segments) {
    const node = segmentToNode(segment);
    if (node) nodes.push(node);
  }
  return nodes;
}

function segmentToNode(segment: InlineSegment): JSONContent | null {
  const value = segment.value.replace(/javascript:/gi, "");
  if (!value) return null;
  if (segment.kind === "text") {
    return { type: "text", text: value };
  }
  if (segment.kind === "bold") {
    return { type: "text", text: value, marks: [{ type: "bold" }] };
  }
  if (segment.kind === "italic") {
    return { type: "text", text: value, marks: [{ type: "italic" }] };
  }
  if (segment.kind === "code") {
    return { type: "text", text: value, marks: [{ type: "code" }] };
  }
  return {
    type: "text",
    text: value,
    marks: [{ type: "link", attrs: { href: segment.href } }],
  };
}
