/**
 * @vitest-environment node
 */
import { describe, expect, it } from "vitest";
import { markdownToTiptapDoc } from "./markdown-adapter";
import { looksLikeHtml } from "./html-adapter";
import {
  containsBase64Image,
  extractOutline,
  headingIdFromText,
  insertBlockAt,
  serializeArticleDoc,
} from "./document";
import { filterSlashCommands } from "./slash-items";
import { isSafeImageSrc } from "@/components/admin/content/block-editor/image-insert-dialog";
import { sanitizeArticleHtml } from "@/lib/public/content-helpers";

describe("markdown to TipTap adapter", () => {
  it("converts headings, paragraphs, lists and code", () => {
    const doc = markdownToTiptapDoc("# Title\n\nHello **world**\n\n- one\n- two\n\n```\nconst x = 1\n```");
    expect(doc.type).toBe("doc");
    expect(doc.content?.[0]).toMatchObject({ type: "heading", attrs: { level: 2 } });
    expect(doc.content?.some((node) => node.type === "bulletList")).toBe(true);
    expect(doc.content?.some((node) => node.type === "codeBlock")).toBe(true);
  });

  it("does not turn javascript links into link marks", () => {
    const doc = markdownToTiptapDoc("[x](javascript:alert(1))");
    const json = serializeArticleDoc(doc);
    expect(json).not.toContain("javascript:");
    expect(json).not.toContain('"type":"link"');
  });
});

describe("article document helpers", () => {
  it("inserts an image between two paragraphs", () => {
    const doc = {
      type: "doc",
      content: [
        { type: "paragraph", content: [{ type: "text", text: "اول" }] },
        { type: "paragraph", content: [{ type: "text", text: "دوم" }] },
      ],
    };
    const next = insertBlockAt(doc, 1, {
      type: "image",
      attrs: { src: "/media/x.jpg", alt: "نمونه" },
    });
    expect(next.content?.map((node) => node.type)).toEqual(["paragraph", "image", "paragraph"]);
    expect(serializeArticleDoc(next)).toContain("/media/x.jpg");
    expect(containsBase64Image(next)).toBe(false);
  });

  it("rejects base64 image sources", () => {
    expect(isSafeImageSrc("data:image/png;base64,abc")).toBe(false);
    expect(isSafeImageSrc("/media/2026/08/cover.png")).toBe(true);
    expect(
      containsBase64Image({
        type: "doc",
        content: [{ type: "image", attrs: { src: "data:image/png;base64,abc" } }],
      }),
    ).toBe(true);
  });

  it("extracts unique outline ids from persian headings", () => {
    const outline = extractOutline({
      type: "doc",
      content: [
        { type: "heading", attrs: { level: 2 }, content: [{ type: "text", text: "مقدمه" }] },
        { type: "heading", attrs: { level: 3 }, content: [{ type: "text", text: "جزئیات" }] },
        { type: "heading", attrs: { level: 2 }, content: [{ type: "text", text: "مقدمه" }] },
      ],
    });
    expect(outline.map((item) => item.text)).toEqual(["مقدمه", "جزئیات", "مقدمه"]);
    expect(outline[0].id).toBe(headingIdFromText("مقدمه", new Set()));
    expect(outline[2].id).not.toBe(outline[0].id);
  });
});

describe("slash menu", () => {
  it("filters commands by keyword in persian and english", () => {
    expect(filterSlashCommands("code").some((item) => item.command === "codeBlock")).toBe(true);
    expect(filterSlashCommands("هشدار").some((item) => item.command === "callout-warning")).toBe(true);
    expect(filterSlashCommands("code").some((item) => item.command === "table")).toBe(false);
  });

  it("includes required block types", () => {
    const all = filterSlashCommands("");
    expect(all.some((item) => item.title === "متن معمولی")).toBe(true);
    expect(all.some((item) => item.command === "table")).toBe(true);
    expect(all.some((item) => item.command === "image")).toBe(true);
    expect(all.some((item) => item.command === "callout-tip")).toBe(true);
  });
});

describe("legacy html detection", () => {
  it("recognizes html bodies", () => {
    expect(looksLikeHtml("<p>سلام</p>")).toBe(true);
    expect(looksLikeHtml("فقط متن ساده")).toBe(false);
  });
});

describe("public HTML sanitizer", () => {
  it("strips script and javascript urls", () => {
    const html = sanitizeArticleHtml('<p onclick="x">ok</p><script>alert(1)</script><a href="javascript:alert(1)">x</a>');
    expect(html).not.toContain("<script");
    expect(html).not.toContain("onclick");
    expect(html).not.toContain("javascript:");
  });
});
