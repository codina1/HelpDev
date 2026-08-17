import { describe, expect, it } from "vitest";
import {
  getPromptLabPack,
  previewPromptLabContent,
  PROMPT_LAB_PACKS,
} from "@/lib/public/prompt-lab-pack-mock";

describe("prompt lab pack mock", () => {
  it("exposes local packs with cover, category, and ordered prompts", () => {
    expect(PROMPT_LAB_PACKS.length).toBeGreaterThan(0);
    for (const pack of PROMPT_LAB_PACKS) {
      expect(pack.coverImage.startsWith("/home/")).toBe(true);
      expect(pack.category.length).toBeGreaterThan(0);
      expect(pack.items.length).toBeGreaterThan(0);
      expect(pack.items.map((item) => item.order)).toEqual(
        pack.items.map((_, index) => index + 1),
      );
    }
  });

  it("loads a pack by slug and previews prompt content", () => {
    const pack = getPromptLabPack("modular-monolith-studio");
    expect(pack).not.toBeNull();
    expect(pack?.title).toContain("Modular Monolith");
    expect(pack?.items[0]?.prompt.slug).toBe("system-boundary-review");
    expect(pack?.items[0]?.preview.length).toBeGreaterThan(0);
    expect(getPromptLabPack("missing-pack")).toBeNull();
    expect(previewPromptLabContent("A\n\nB\nC\nD\nE", 3)).toBe("A\nB\nC");
  });
});
