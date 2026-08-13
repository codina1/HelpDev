import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { homeColors, homeContainer, homeTokens } from "@/lib/design-system/home-tokens";

describe("homepage design tokens", () => {
  const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");

  it("defines --home-* variables without replacing --ds-* or --pub-*", () => {
    expect(css).toContain("--home-bg:");
    expect(css).toContain("--home-surface:");
    expect(css).toContain("--home-border:");
    expect(css).toContain("--home-purple:");
    expect(css).toContain("--home-blue:");
    expect(css).toContain("--home-cyan:");
    expect(css).toContain("--home-text:");
    expect(css).toContain("--home-radius-xl:");
    expect(css).toContain("--home-glow-purple:");
    expect(css).toContain("--home-section-gap:");
    expect(css).toContain("--home-container:");
    expect(css).toContain("--home-display-size:");
    expect(css).toContain("--ds-bg: #060816");
    expect(css).toContain("--pub-bg: var(--ds-bg)");
    expect(css).toContain("html:not(.dark)");
  });

  it("mirrors the dark navy + purple/blue/cyan homepage palette", () => {
    expect(homeColors.background.toLowerCase()).toBe("#060816");
    expect(homeColors.purple.toLowerCase()).toBe("#8b5cf6");
    expect(homeColors.blue.toLowerCase()).toBe("#6366f1");
    expect(homeColors.cyan.toLowerCase()).toBe("#06b6d4");
    expect(homeContainer.default).toBe("100%");
    expect(homeContainer.headerHeight).toBe("72px");
    expect(homeTokens.typography.fontFamily).toContain("--font-vazirmatn");
  });
});
