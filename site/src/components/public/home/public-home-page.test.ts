import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("public homepage composition", () => {
  const source = readFileSync(
    join(process.cwd(), "src/components/public/home/public-home-page.tsx"),
    "utf8",
  );

  it("follows the design-reference section order", () => {
    const order = [
      "HomeHero",
      "HomeSearchSection",
      "HomeStatsSection",
      "HomeQuickAccessSection",
      "HomeHubSection",
      "HomeCategoriesSection",
      "HomeArticlesSection",
      "HomeNewsSection",
      "HomePathsSection",
      "HomeNewsletterSection",
    ];

    let cursor = -1;
    for (const name of order) {
      const index = source.indexOf(name);
      expect(index, name).toBeGreaterThan(cursor);
      cursor = index;
    }

    expect(source).not.toContain("HomeWorkflowSection");
    expect(source).not.toContain("HomeValueSection");
    expect(source).not.toContain("HomeTrustSection");
    expect(source).not.toContain("HomeStatsStrip");
    expect(source).not.toContain("PersonalizedHero");
  });
});
