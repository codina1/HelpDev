import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("homepage mobile responsive rules", () => {
  const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");
  const hero = readFileSync(join(process.cwd(), "src/components/public/home/HeroSection.tsx"), "utf8");
  const header = readFileSync(join(process.cwd(), "src/components/public/public-header.tsx"), "utf8");
  const workspace = readFileSync(
    join(process.cwd(), "src/components/public/home/home-hero-workspace.tsx"),
    "utf8",
  );

  it("contains overflow containment and mobile breakpoints", () => {
    expect(css).toContain("overflow-x: clip");
    expect(css).toContain("max-width: 430px");
    expect(css).toContain(".home-hero-float");
    expect(css).toContain(".home-path-item");
    expect(css).toContain(".pub-footer-grid");
  });

  it("keeps hero stacking and a responsive workspace", () => {
    expect(hero).toContain("lg:grid-cols-2");
    expect(hero).toContain("lg:min-h-[650px]");
    expect(hero).toContain("HomeHeroWorkspace");
    expect(hero).toContain("order-1");
    expect(workspace).toContain("home-hero-float");
  });

  it("keeps 64px header mobile chrome", () => {
    expect(header).toContain("h-[64px]");
    expect(header).toContain("pub-navbar-search");
    expect(header).toContain("lg:hidden");
    expect(header).toContain("rounded-xl");
  });
});
