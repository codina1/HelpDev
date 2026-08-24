/**
 * @vitest-environment node
 */
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

const EDITOR = join(process.cwd(), "src/components/admin/content/block-editor/article-block-editor.tsx");
const RICH = join(process.cwd(), "src/components/admin/content/block-editor/article-rich-text-editor.tsx");
const EXTENSIONS = join(process.cwd(), "src/components/admin/content/block-editor/extensions.ts");
const EDIT_VIEW = join(process.cwd(), "src/components/admin/content/editor/content-edit-view.tsx");

describe("article block editor wiring", () => {
  it("is RTL and uses the shared TipTap editor", () => {
    const source = readFileSync(EDITOR, "utf8");
    const rich = readFileSync(RICH, "utf8");
    expect(source).toContain("ArticleRichTextEditor");
    expect(source).toContain("autosave: true");
    expect(source).toContain("beforeunload");
    expect(source).toContain("saveDraft");
    expect(source).toContain("previewArticleContent");
    expect(source).toContain("MediaPickerDialog");
    expect(rich).toContain('dir: "rtl"');
    expect(rich).toContain("useEditor");
    expect(rich).toContain("handlePaste");
    expect(rich).toContain("handleDrop");
    expect(rich).toContain("SlashCommandMenu");
  });

  it("registers Gutenberg-style blocks including code, table, callout and media", () => {
    const source = readFileSync(EXTENSIONS, "utf8");
    expect(source).toContain('name: "callout"');
    expect(source).toContain('name: "terminal"');
    expect(source).toContain('name: "gallery"');
    expect(source).toContain('name: "cta"');
    expect(source).toContain("Table.configure");
    expect(source).toContain("Youtube.configure");
    expect(source).toContain("CodeBlockLowlight");
  });

  it("uses the block editor only for articles", () => {
    const source = readFileSync(EDIT_VIEW, "utf8");
    expect(source).toContain('data.type === "Article"');
    expect(source).toContain("ArticleBlockEditor");
    expect(source).toContain("ContentStudio");
  });

  it("keeps keyboard shortcuts for save, duplicate and move", () => {
    const rich = readFileSync(RICH, "utf8");
    expect(rich).toContain('event.key.toLowerCase() === "s"');
    expect(rich).toContain('event.key.toLowerCase() === "d"');
    expect(rich).toContain('event.key === "ArrowUp"');
    expect(rich).toContain("duplicateSelectedBlock");
  });

  it("remaps public text tokens so preview contrast matches the admin surface", () => {
    const css = readFileSync(join(process.cwd(), "src/components/admin/content/block-editor/article-block-editor.module.css"), "utf8");
    expect(css).toContain("--pub-fg: var(--adm-text)");
    expect(css).toContain("color: var(--adm-text)");
  });
});
