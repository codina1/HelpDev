import { describe, expect, it } from "vitest";
import { parseInline, parseMarkdown } from "./markdown";

describe("parseMarkdown", () => {
  it("parses headings with levels 1-3", () => {
    const blocks = parseMarkdown("# One\n## Two\n### Three");
    expect(blocks).toEqual([
      { kind: "heading", level: 1, text: "One" },
      { kind: "heading", level: 2, text: "Two" },
      { kind: "heading", level: 3, text: "Three" },
    ]);
  });

  it("groups consecutive lines into paragraphs separated by blank lines", () => {
    const blocks = parseMarkdown("line a\nline b\n\nsecond para");
    expect(blocks).toEqual([
      { kind: "paragraph", text: "line a line b" },
      { kind: "paragraph", text: "second para" },
    ]);
  });

  it("parses fenced code blocks verbatim", () => {
    const blocks = parseMarkdown("```\nconst x = 1;\nconst y = 2;\n```");
    expect(blocks).toEqual([{ kind: "code", text: "const x = 1;\nconst y = 2;" }]);
  });

  it("parses unordered and ordered lists", () => {
    const unordered = parseMarkdown("- a\n- b");
    expect(unordered).toEqual([{ kind: "list", ordered: false, items: ["a", "b"] }]);
    const ordered = parseMarkdown("1. a\n2. b");
    expect(ordered).toEqual([{ kind: "list", ordered: true, items: ["a", "b"] }]);
  });

  it("returns an empty array for empty input", () => {
    expect(parseMarkdown("")).toEqual([]);
  });
});

describe("parseInline", () => {
  it("parses bold, italic and inline code", () => {
    expect(parseInline("**bold**")).toEqual([{ kind: "bold", value: "bold" }]);
    expect(parseInline("*italic*")).toEqual([{ kind: "italic", value: "italic" }]);
    expect(parseInline("`code`")).toEqual([{ kind: "code", value: "code" }]);
  });

  it("parses safe links and keeps surrounding text", () => {
    expect(parseInline("see [docs](https://x.dev) now")).toEqual([
      { kind: "text", value: "see " },
      { kind: "link", value: "docs", href: "https://x.dev" },
      { kind: "text", value: " now" },
    ]);
  });

  it("treats unsafe link protocols as plain text (no XSS)", () => {
    const segments = parseInline("[x](javascript:alert(1))");
    expect(segments.every((s) => s.kind === "text")).toBe(true);
  });

  it("prefers bold over italic for double asterisks", () => {
    expect(parseInline("**strong**")).toEqual([{ kind: "bold", value: "strong" }]);
  });
});
