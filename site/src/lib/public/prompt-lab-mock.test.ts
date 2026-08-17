import { describe, expect, it } from "vitest";
import {
  PROMPT_LAB_CATEGORIES,
  PROMPT_LAB_PROMPTS,
  featuredPromptLabPrompts,
  filterPromptLabPrompts,
  latestPromptLabPrompts,
  popularPromptLabPrompts,
} from "@/lib/public/prompt-lab-mock";

describe("prompt lab mock catalog", () => {
  it("exposes the required Persian categories", () => {
    expect(PROMPT_LAB_CATEGORIES.map((item) => item.name)).toEqual([
      "تصویر",
      "ویدئو",
      "کدنویسی",
      "تولید محتوا",
      "طراحی",
      "مارکتینگ",
      "آموزش",
    ]);
  });

  it("keeps typed local cards with cover, model, category, and counts", () => {
    expect(PROMPT_LAB_PROMPTS.length).toBeGreaterThan(0);
    for (const prompt of PROMPT_LAB_PROMPTS) {
      expect(prompt.coverImage.startsWith("/home/")).toBe(true);
      expect(prompt.title.length).toBeGreaterThan(0);
      expect(prompt.description.length).toBeGreaterThan(0);
      expect(prompt.aiModel.length).toBeGreaterThan(0);
      expect(prompt.category.length).toBeGreaterThan(0);
      expect(prompt.copyCount).toBeGreaterThanOrEqual(0);
      expect(prompt.viewCount).toBeGreaterThanOrEqual(0);
    }
  });

  it("selects featured, popular, and latest subsets from local data", () => {
    const featured = featuredPromptLabPrompts(PROMPT_LAB_PROMPTS);
    const popular = popularPromptLabPrompts(PROMPT_LAB_PROMPTS);
    const latest = latestPromptLabPrompts(PROMPT_LAB_PROMPTS);
    expect(featured.every((item) => item.featured)).toBe(true);
    expect(popular[0]?.viewCount).toBeGreaterThanOrEqual(popular[1]?.viewCount ?? 0);
    expect(Date.parse(latest[0]?.publishedAt ?? "")).toBeGreaterThanOrEqual(
      Date.parse(latest[1]?.publishedAt ?? ""),
    );
  });

  it("filters locally without an API", () => {
    const coding = filterPromptLabPrompts(PROMPT_LAB_PROMPTS, "", "coding");
    expect(coding.length).toBeGreaterThan(0);
    expect(coding.every((item) => item.categorySlug === "coding")).toBe(true);
    const search = filterPromptLabPrompts(PROMPT_LAB_PROMPTS, "RAG");
    expect(search.some((item) => item.slug === "rag-query-rewrite")).toBe(true);
  });
});
