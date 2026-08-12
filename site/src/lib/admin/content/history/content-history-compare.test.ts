import { describe, expect, it } from "vitest";
import { compareRevisionSnapshots, countChangedFields } from "./history-compare";
import type { ContentRevisionSnapshot } from "./history-types";

function snapshot(overrides: Partial<ContentRevisionSnapshot> = {}): ContentRevisionSnapshot {
  return {
    title: "Title A",
    slug: "title-a",
    body: "Body text",
    excerpt: "Excerpt",
    coverImage: null,
    contentType: "Article",
    seoMetadata: {
      seoTitle: "SEO",
      seoDescription: "Desc",
      canonicalUrl: null,
      ogImage: null,
      focusKeyword: "kw",
    },
    ...overrides,
  };
}

describe("compareRevisionSnapshots", () => {
  it("marks identical snapshots as unchanged", () => {
    const left = snapshot();
    const right = snapshot();
    const fields = compareRevisionSnapshots(left, right);
    expect(countChangedFields(fields)).toBe(0);
    expect(fields.every((f) => !f.changed)).toBe(true);
  });

  it("detects title and SEO diffs", () => {
    const left = snapshot();
    const right = snapshot({
      title: "Title B",
      seoMetadata: { ...left.seoMetadata, focusKeyword: "other" },
    });
    const fields = compareRevisionSnapshots(left, right);
    const changed = fields.filter((f) => f.changed).map((f) => f.key);
    expect(changed).toContain("title");
    expect(changed).toContain("focusKeyword");
    expect(countChangedFields(fields)).toBe(2);
  });

  it("normalizes whitespace for comparison", () => {
    const left = snapshot({ body: "hello" });
    const right = snapshot({ body: "  hello  " });
    const titleField = compareRevisionSnapshots(left, right).find((f) => f.key === "body");
    expect(titleField?.changed).toBe(false);
  });
});
