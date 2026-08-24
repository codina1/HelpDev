import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

describe("homepage hero", () => {
  it("renders platform copy, CTAs, search, chips, and stats", () => {
    const html = renderToStaticMarkup(<HomeHero />);
    expect(html).toContain("سیستم عامل رشد");
    expect(html).toContain("توسعه‌دهندگان در عصر AI");
    expect(html).toContain("یاد بگیر، ابزار بساز و سریع‌تر توسعه بده");
    expect(html).toContain("شروع مسیر");
    expect(html).toContain("کاوش HelpDev");
    expect(html).toContain("/learning");
    expect(html).toContain("/articles");
    expect(html).toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(html).toContain("MCP");
    expect(html).toContain("Cursor");
    expect(html).toContain("Claude Code");
    expect(html).toContain(".NET");
    expect(html).toContain("React");
    expect(html).toContain("Python");
    expect(html).toContain("DevOps");
    expect(html).toContain("AI Agent");
    expect(html).toContain("مقاله آموزشی");
    expect(html).toContain("Prompt آماده");
    expect(html).toContain("ابزار کاربردی");
    expect(html).toContain("توسعه‌دهنده");
  });

  it("renders the developer workspace with floating cards", () => {
    const html = renderToStaticMarkup(<HomeHeroWorkspace />);
    expect(html).toContain("AI Assistant");
    expect(html).toContain("Prompt Card");
    expect(html).toContain("Tools Card");
    expect(html).toContain("Code Card");
    expect(html).toContain("helpdev — workspace");
    expect(html).toContain("home-hero-float");
  });
});
