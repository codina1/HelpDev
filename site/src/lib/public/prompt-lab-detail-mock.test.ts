import { describe, expect, it } from "vitest";
import {
  getPromptLabDetail,
  relatedPromptLabPrompts,
  similarPromptLabPrompts,
} from "@/lib/public/prompt-lab-detail-mock";

describe("prompt lab detail mock", () => {
  it("loads a local detail by slug with author, content, and tags", () => {
    const detail = getPromptLabDetail("system-boundary-review");
    expect(detail).not.toBeNull();
    expect(detail?.title).toContain("مرز ماژول");
    expect(detail?.author.name).toBe("نیما رضایی");
    expect(detail?.content.length).toBeGreaterThan(40);
    expect(detail?.tags.length).toBeGreaterThan(0);
    expect(detail?.mediaType).toBe("Text");
    expect(getPromptLabDetail("missing-slug")).toBeNull();
  });

  it("returns related and similar items that exclude the current slug", () => {
    const related = relatedPromptLabPrompts("system-boundary-review");
    const similar = similarPromptLabPrompts("system-boundary-review");
    expect(related.length).toBeGreaterThan(0);
    expect(similar.length).toBeGreaterThan(0);
    expect(related.every((item) => item.slug !== "system-boundary-review")).toBe(true);
    expect(similar.every((item) => item.slug !== "system-boundary-review")).toBe(true);
    expect(related.some((item) => item.categorySlug === "coding")).toBe(true);
  });
});
