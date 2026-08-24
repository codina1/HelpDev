import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("homepage mobile responsive rules", () => {
  const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");
  const hero = readFileSync(join(process.cwd(), "src/components/public/home/home-hero.tsx"), "utf8");
  const header = readFileSync(join(process.cwd(), "src/components/public/public-header.tsx"), "utf8");
  const workspace = readFileSync(
    join(process.cwd(), "src/components/public/home/home-hero-workspace.tsx"),
    "utf8",
  );

  it("contains overflow containment and the requested mobile widths", () => {
    expect(css).toContain("overflow-x: clip");
    expect(css).toContain("max-width: 430px");
    expect(css).toContain("max-width: 374px");
    expect(css).toContain("max-width: 359px");
    expect(css).toContain(".home-hero-float");
    expect(css).toContain(".home-workflow-panel");
    expect(css).toContain(".home-path-item");
    expect(css).toContain(".home-articles-scroller");
    expect(css).toContain(".pub-footer-grid");
  });

  it("keeps hero stacking and a responsive workspace on small screens", () => {
    expect(hero).toContain("lg:grid-cols-2");
    expect(hero).toContain("HomeHeroWorkspace");
    expect(workspace).toContain("max-w-[min(100%,22rem)]");
    expect(workspace).toContain("home-hero-float");
  });

  it("keeps mobile header actions without removing routes", () => {
    expect(header).toContain("pub-navbar-wordmark");
    expect(header).toContain("pub-navbar-search");
    expect(header).toContain("lg:hidden");
    expect(header).toContain("باز کردن منو");
  });
});
