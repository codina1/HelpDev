import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HOME_TOPIC_CHIPS, HomeSearchSection } from "@/components/public/home/home-search-section";
import { HOME_PLATFORM_STATS, HomeStatsSection } from "@/components/public/home/home-stats-section";
import { HomeHubSection } from "@/components/public/home/home-hub-section";
import { HOME_CONTENT_CATEGORIES, HomeCategoriesSection } from "@/components/public/home/home-categories-section";
import { HomeNewsSection } from "@/components/public/home/home-news-section";
import { HOME_PATH_ITEMS } from "@/components/public/home/home-paths-section";

describe("homepage platform sections", () => {
  it("renders search placeholder and topic chips in reference order", () => {
    const html = renderToStaticMarkup(<HomeSearchSection />);
    expect(html).toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(HOME_TOPIC_CHIPS).toEqual([
      "MCP",
      "Claude Code",
      "Cursor",
      ".NET",
      "React",
      "Python",
      "DevOps",
      "AI Agent",
    ]);
    for (const chip of HOME_TOPIC_CHIPS) {
      expect(html).toContain(chip);
    }
  });

  it("renders four platform stats", () => {
    const html = renderToStaticMarkup(<HomeStatsSection />);
    expect(HOME_PLATFORM_STATS).toHaveLength(5);
    for (const stat of HOME_PLATFORM_STATS) {
      expect(html).toContain(stat.value);
      expect(html).toContain(stat.label);
    }
  });

  it("renders toolbox and prompt lab hub columns", () => {
    const html = renderToStaticMarkup(<HomeHubSection />);
    expect(html).toContain("Developer Toolbox");
    expect(html).toContain("Prompt Lab");
    expect(html).toContain("/toolbox");
    expect(html).toContain("/prompt-lab");
    expect(html).toContain("/home/icon-prompt-lab.png");
    expect(html).toContain("/home/icon-jwt.png");
  });

  it("renders eight content categories", () => {
    const html = renderToStaticMarkup(<HomeCategoriesSection />);
    expect(HOME_CONTENT_CATEGORIES).toHaveLength(8);
    for (const category of HOME_CONTENT_CATEGORIES) {
      expect(html).toContain(category.label);
      expect(html).toContain(category.icon);
    }
  });

  it("renders compact news cards", () => {
    const html = renderToStaticMarkup(<HomeNewsSection />);
    expect(html).toContain("آخرین اخبار");
    expect(html).toContain("/news");
  });

  it("keeps the five compact learning path roles", () => {
    expect(HOME_PATH_ITEMS.map((item) => item.title)).toEqual([
      "Frontend Developer",
      "DevOps Engineer",
      ".NET Developer",
      "Backend Developer",
      "AI Engineer",
    ]);
  });
});
