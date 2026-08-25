import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

describe("homepage hero", () => {
  it("renders platform copy and CTAs without search/stats", () => {
    const html = renderToStaticMarkup(<HomeHero />);
    expect(html).toContain("سیستم عامل رشد");
    expect(html).toContain("توسعه‌دهندگان در عصر AI");
    expect(html).toContain("یاد بگیر، ابزار بساز و سریع‌تر توسعه بده");
    expect(html).toContain("HelpDev مجموعه‌ای از آموزش‌ها");
    expect(html).toContain("شروع مسیر");
    expect(html).toContain("کاوش HelpDev");
    expect(html).toContain("/learning");
    expect(html).toContain("/articles");
    expect(html).toContain("min-h-[560px]");
    expect(html).toContain("lg:text-[56px]");
    expect(html).toContain("lg:grid-cols-2");
    expect(html).not.toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(html).not.toContain("مقاله آموزشی");
  });

  it("stacks illustration above copy on mobile", () => {
    const html = renderToStaticMarkup(<HomeHero />);
    expect(html).toContain("order-1");
    expect(html).toContain("order-2");
    expect(html).toContain("lg:order-1");
    expect(html).toContain("lg:order-2");
  });

  it("renders the developer workspace with floating cards", () => {
    const html = renderToStaticMarkup(<HomeHeroWorkspace />);
    expect(html).toContain("AI Assistant");
    expect(html).toContain("Prompt Card");
    expect(html).toContain("Tools Card");
    expect(html).toContain("Code Card");
    expect(html).toContain("home-hero-float");
  });
});
