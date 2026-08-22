import type { JSONContent } from "@tiptap/core";
import { generateJSON } from "@tiptap/html";
import { createArticleExtensions } from "@/components/admin/content/block-editor/extensions";
import { markdownToTiptapDoc } from "@/lib/admin/content/block-editor/markdown-adapter";
import { EMPTY_ARTICLE_DOC, parseArticleDoc } from "@/lib/admin/content/block-editor/document";

export function looksLikeHtml(value: string): boolean {
  return /<\/?[a-z][\s\S]*>/i.test(value.trim());
}

/**
 * Loads a previous article body into TipTap JSON without mutating stored data.
 * JSON documents win, then HTML via the official parser, then Markdown/plain text.
 */
export function legacyBodyToTiptapDoc(body: string | null | undefined): JSONContent {
  const raw = body?.trim() ?? "";
  if (!raw) return EMPTY_ARTICLE_DOC;

  const asJson = parseArticleDoc(raw);
  if (asJson) return asJson;

  if (looksLikeHtml(raw)) {
    try {
      return generateJSON(raw, createArticleExtensions()) as JSONContent;
    } catch {
      return markdownToTiptapDoc(raw);
    }
  }

  return markdownToTiptapDoc(raw);
}
