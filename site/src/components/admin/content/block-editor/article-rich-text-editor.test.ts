/**
 * @vitest-environment node
 */
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

const ROOT = join(process.cwd(), "src/components/admin/content/block-editor");
const LIB = join(process.cwd(), "src/lib/admin/content/block-editor");

describe("article rich text editor", () => {
  it("exports a reusable ArticleRichTextEditor used by create and edit", () => {
    const editor = readFileSync(join(ROOT, "article-rich-text-editor.tsx"), "utf8");
    const page = readFileSync(join(ROOT, "article-block-editor.tsx"), "utf8");
    const create = readFileSync(
      join(process.cwd(), "src/components/admin/content/workspaces/article/article-editor.tsx"),
      "utf8",
    );
    const edit = readFileSync(
      join(process.cwd(), "src/components/admin/content/editor/content-edit-view.tsx"),
      "utf8",
    );

    expect(editor).toContain("export type ArticleRichTextEditorProps");
    expect(editor).toContain("value: EditorContent");
    expect(editor).toContain("onChange: (content: EditorContent) => void");
    expect(editor).toContain('dir: "rtl"');
    expect(editor).toContain("useEditor");
    expect(page).toContain("ArticleRichTextEditor");
    expect(create).toContain("ArticleBlockEditor");
    expect(edit).toContain("ArticleBlockEditor");
    expect(edit).toContain('data.type === "Article"');
  });

  it("keeps a full toolbar, slash menu, bubble menu and status bar", () => {
    const editor = readFileSync(join(ROOT, "article-rich-text-editor.tsx"), "utf8");
    const toolbar = readFileSync(join(ROOT, "editor-toolbar.tsx"), "utf8");
    expect(editor).toContain("EditorToolbar");
    expect(editor).toContain("EditorStatusBar");
    expect(editor).toContain("SlashCommandMenu");
    expect(editor).toContain("FloatingTextToolbar");
    expect(editor).toContain("handlePaste");
    expect(editor).toContain("handleDrop");
    expect(toolbar).toContain("H2");
    expect(toolbar).toContain("جدول");
    expect(toolbar).toContain("تمام‌صفحه");
    expect(toolbar).toContain("Ctrl+K");
  });

  it("registers only free TipTap extensions including underline highlight color and tables", () => {
    const source = readFileSync(join(ROOT, "extensions.ts"), "utf8");
    expect(source).toContain("Underline");
    expect(source).toContain("Highlight");
    expect(source).toContain("Color");
    expect(source).toContain("TextStyle");
    expect(source).toContain("Table.configure");
    expect(source).toContain("CodeBlockLowlight");
    expect(source).toContain('name: "callout"');
    expect(source).toContain("allowBase64: false");
    expect(source).not.toContain("tiptap.dev/cloud");
    expect(source).not.toContain("collaboration");
  });

  it("does not store base64 images and supports media ids", () => {
    const source = readFileSync(join(ROOT, "extensions.ts"), "utf8");
    const dialog = readFileSync(join(ROOT, "image-insert-dialog.tsx"), "utf8");
    expect(source).toContain("mediaId");
    expect(dialog).toContain("base64");
    expect(dialog).toContain("isSafeImageSrc");
  });
});

describe("legacy article loading", () => {
  it("detects html bodies separately from markdown", () => {
    const source = readFileSync(join(LIB, "html-adapter.ts"), "utf8");
    expect(source).toContain("looksLikeHtml");
    expect(source).toContain("generateJSON");
    expect(source).toContain("markdownToTiptapDoc");
  });
});
