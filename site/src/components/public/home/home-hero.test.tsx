import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HeroSection } from "@/components/public/home/HeroSection";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

describe("homepage hero", () => {
  it("renders fluid responsive hero with search chips and freestanding stats", () => {
    const html = renderToStaticMarkup(<HeroSection />);
    expect(html).toContain("سیستم عامل رشد");
    expect(html).toContain("توسعه‌دهندگان در عصر AI");
    expect(html).toContain("whitespace-normal");
    expect(html).toContain("یاد بگیر، ابزار بساز و سریع‌تر توسعه بده");
    expect(html).toContain("شروع مسیر");
    expect(html).toContain("کاوش HelpDev");
    expect(html).toContain("min-[1440px]:min-h-[560px]");
    expect(html).toContain("max-w-[1280px]");
    expect(html).toContain("lg:grid-cols-2");
    expect(html).toContain("text-[32px]");
    expect(html).toContain("sm:text-[42px]");
    expect(html).toContain("min-[1440px]:text-[52px]");
    expect(html).toContain("grid-cols-2");
    expect(html).toContain("sm:grid-cols-4");
    expect(html).toContain("max-w-[28rem]");
    expect(html).toContain("h-[50px]");
    expect(html).toContain("mt-6");
    expect(html).toContain("هر چیزی که می‌خواهی جستجو کن");
    expect(html).toContain("مقاله آموزشی");
    expect(html).toContain("+1200");
    expect(html).not.toContain("lg:w-[650px]");
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

  it("renders the reference 3D workspace illustration", () => {
    const html = renderToStaticMarkup(<HomeHeroWorkspace />);
    expect(html).toContain("/home/hero-workspace.webp");
    expect(html).toContain("home-hero-float-slow");
    expect(html).toContain("max-w-[320px]");
    expect(html).toContain("sm:max-w-[450px]");
    expect(html).toContain("min-[1440px]:max-w-[620px]");
    expect(html).toContain("object-contain");
  });
});
