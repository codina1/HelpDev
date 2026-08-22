import type { JSONContent } from "@tiptap/core";

export const ARTICLE_EDITOR_VERSION = "1";
export const ARTICLE_CONTENT_FORMAT = "blocks";

export const EMPTY_ARTICLE_DOC: JSONContent = {
  type: "doc",
  content: [{ type: "paragraph" }],
};

export function parseArticleDoc(raw: string | null | undefined): JSONContent | null {
  if (!raw || !raw.trim()) return null;
  try {
    const parsed = JSON.parse(raw) as JSONContent;
    if (parsed && parsed.type === "doc") return parsed;
    return null;
  } catch {
    return null;
  }
}

export function serializeArticleDoc(doc: JSONContent): string {
  return JSON.stringify(doc);
}

export function isEmptyArticleDoc(doc: JSONContent | null | undefined): boolean {
  if (!doc?.content?.length) return true;
  return !extractPlainText(doc).trim();
}

export function extractPlainText(node: JSONContent | null | undefined): string {
  if (!node) return "";
  if (typeof node.text === "string") return node.text;
  if (!node.content?.length) return "";
  return node.content.map((child) => extractPlainText(child)).join(node.type === "paragraph" ? "\n" : " ");
}

export function countWords(text: string): number {
  return text
    .trim()
    .split(/\s+/)
    .filter(Boolean).length;
}

export function estimateReadingMinutes(wordCount: number): number {
  return Math.max(1, Math.ceil(wordCount / 200));
}

export type ArticleOutlineItem = {
  id: string;
  level: 2 | 3 | 4;
  text: string;
};

export function extractOutline(doc: JSONContent | null | undefined): ArticleOutlineItem[] {
  const items: ArticleOutlineItem[] = [];
  walk(doc, (node) => {
    if (node.type !== "heading") return;
    const level = Number(node.attrs?.level);
    if (level !== 2 && level !== 3 && level !== 4) return;
    const text = extractPlainText(node).trim();
    if (!text) return;
    items.push({ id: `h-${items.length + 1}`, level, text });
  });
  return items;
}

export function insertBlockAt(
  doc: JSONContent,
  index: number,
  block: JSONContent,
): JSONContent {
  const content = [...(doc.content ?? [])];
  const clamped = Math.max(0, Math.min(index, content.length));
  content.splice(clamped, 0, block);
  return { ...doc, content };
}

function walk(node: JSONContent | null | undefined, visit: (node: JSONContent) => void): void {
  if (!node) return;
  visit(node);
  for (const child of node.content ?? []) walk(child, visit);
}
