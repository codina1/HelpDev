import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HeroSection } from "@/components/public/home/HeroSection";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

describe("homepage hero", () => {
  it("renders compact platform hero with search chips and freestanding stats", () => {
    const html = renderToStaticMarkup(<HeroSection />);
    expect(html).toContain("سیستم عامل رشد");
    expect(html).toContain("توسعه‌دهندگان در عصر AI");
    expect(html).toContain("whitespace-nowrap");
    expect(html).toContain("یاد بگیر، ابزار بساز و سریع‌تر توسعه بده");
    expect(html).toContain("شروع مسیر");
    expect(html).toContain("کاوش HelpDev");
    expect(html).toContain("lg:h-[560px]");
    expect(html).toContain("max-w-[1280px]");
    expect(html).toContain("lg:text-[52px]");
    expect(html).toContain("max-w-[500px]");
    expect(html).toContain("lg:w-[430px]");
    expect(html).toContain("h-[52px]");
    expect(html).toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(html).toContain("مقاله آموزشی");
    expect(html).toContain("+1200");
    expect(html).toContain("gap-x-10");
    expect(html).not.toContain("sm:bg-[#0B1224]/70");
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

  it("renders a larger 3D workspace with corner floating cards", () => {
    const html = renderToStaticMarkup(<HomeHeroWorkspace />);
    expect(html).toContain("AI Assistant");
    expect(html).toContain("Cursor");
    expect(html).toContain("JSON · JWT · Regex");
    expect(html).toContain("CODE");
    expect(html).toContain("home-hero-float");
    expect(html).toContain("lg:h-[520px]");
    expect(html).toContain("lg:w-[620px]");
    expect(html).toContain("rounded-[18px]");
    expect(html).toContain("bg-[rgba(15,23,42,0.8)]");
    expect(html).toContain("left-0 top-[2%]");
    expect(html).toContain("right-0 top-[4%]");
  });
});
