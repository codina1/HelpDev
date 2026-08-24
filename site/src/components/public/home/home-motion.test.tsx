import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeHero } from "@/components/public/home/home-hero";
import { HomeWorkflowSection } from "@/components/public/home/home-workflow-section";
import { HomeValueSection } from "@/components/public/home/home-value-section";

describe("homepage premium motion", () => {
  const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");
  const observer = readFileSync(
    join(process.cwd(), "src/components/public/home/v2/interaction-reveal-observer.tsx"),
    "utf8",
  );

  it("defines CSS-only float, glow, and reveal motion", () => {
    expect(css).toContain("@keyframes hero-float");
    expect(css).toContain(".home-hero-float");
    expect(css).toContain(".home-reveal");
    expect(css).toContain("prefers-reduced-motion: reduce");
    expect(observer).toContain(".home-reveal");
  });

  it("marks hero workspace motion and below-fold reveal sections", () => {
    const hero = renderToStaticMarkup(<HomeHero />);
    expect(hero).toContain("home-hero-workspace");
    expect(hero).toContain("home-hero-float");
    expect(hero).toContain("سیستم عامل رشد");
    expect(renderToStaticMarkup(<HomeWorkflowSection />)).toContain("home-reveal");
    expect(renderToStaticMarkup(<HomeValueSection />)).toContain("home-reveal");
  });
});
