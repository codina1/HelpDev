import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HeroSection } from "@/components/public/home/HeroSection";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

describe("homepage hero", () => {
  it("renders platform copy, CTAs, search, chips and stats", () => {
    const html = renderToStaticMarkup(<HeroSection />);
    expect(html).toContain("سیستم عامل رشد");
    expect(html).toContain("توسعه‌دهندگان در عصر AI");
    expect(html).toContain("یاد بگیر، ابزار بساز و سریع‌تر توسعه بده");
    expect(html).toContain("HelpDev مجموعه‌ای از آموزش‌ها");
    expect(html).toContain("شروع مسیر");
    expect(html).toContain("کاوش HelpDev");
    expect(html).toContain("/learning");
    expect(html).toContain("/articles");
    expect(html).toContain("lg:min-h-[650px]");
    expect(html).toContain("lg:text-[56px]");
    expect(html).toContain("from-[#7C3AED] to-[#2563EB]");
    expect(html).toContain("lg:grid-cols-2");
    expect(html).toContain("lg:w-[420px]");
    expect(html).toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(html).toContain("مقاله آموزشی");
    expect(html).toContain("Claude Code");
    expect(html).toContain("+1200");
  });

  it("keeps HomeHero as a thin alias of HeroSection", () => {
    const a = renderToStaticMarkup(<HeroSection />);
    const b = renderToStaticMarkup(<HomeHero />);
    expect(a).toBe(b);
  });

  it("stacks illustration above copy on mobile", () => {
    const html = renderToStaticMarkup(<HeroSection />);
    expect(html).toContain("order-1");
    expect(html).toContain("order-2");
    expect(html).toContain("lg:order-1");
    expect(html).toContain("lg:order-2");
  });

  it("renders the developer workspace with floating cards", () => {
    const html = renderToStaticMarkup(<HomeHeroWorkspace />);
    expect(html).toContain("AI Assistant");
    expect(html).toContain("Cursor");
    expect(html).toContain("JSON · JWT · Regex");
    expect(html).toContain("CODE");
    expect(html).toContain("home-hero-float");
  });
});
