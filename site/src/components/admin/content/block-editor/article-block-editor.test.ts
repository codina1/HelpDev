/**
 * @vitest-environment node
 */
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

const EDITOR = join(process.cwd(), "src/components/admin/content/block-editor/article-block-editor.tsx");
const EXTENSIONS = join(process.cwd(), "src/components/admin/content/block-editor/extensions.ts");
const EDIT_VIEW = join(process.cwd(), "src/components/admin/content/editor/content-edit-view.tsx");

describe("article block editor wiring", () => {
  it("is RTL and uses TipTap", () => {
    const source = readFileSync(EDITOR, "utf8");
    expect(source).toContain('dir: "rtl"');
    expect(source).toContain("useEditor");
    expect(source).toContain("autosave: true");
    expect(source).toContain("beforeunload");
    expect(source).toContain("saveDraft");
    expect(source).toContain("previewArticleContent");
    expect(source).toContain("MediaPickerDialog");
    expect(source).toContain("handlePaste");
    expect(source).toContain("handleDrop");
    expect(source).toContain("SlashCommandMenu");
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
    const source = readFileSync(EDITOR, "utf8");
    expect(source).toContain('event.key.toLowerCase() === "s"');
    expect(source).toContain('event.key.toLowerCase() === "d"');
    expect(source).toContain('event.key === "ArrowUp"');
    expect(source).toContain("duplicateSelectedBlock");
  });
});
