"use client";

import { ArticleBlockEditor } from "@/components/admin/content/block-editor/article-block-editor";

/** Article creation uses the Gutenberg-style TipTap block editor. */
export function ArticleEditor() {
  return <ArticleBlockEditor />;
}
