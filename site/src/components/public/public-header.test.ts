import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("public homepage header", () => {
  const source = readFileSync(join(process.cwd(), "src/components/public/public-header.tsx"), "utf8");

  it("keeps RTL glass chrome with logo, nav, AI entry, actions, and login", () => {
    expect(source).toContain('href="/"');
    expect(source).toContain("SITE.name");
    expect(source).toContain("PUBLIC_PRODUCTS_NAV");
    expect(source).toContain("از AI بپرس");
    expect(source).toContain("ورود");
    expect(source).toContain("ThemeToggle");
    expect(source).toContain("helpdev-public-theme");
    expect(source).toContain('classList.toggle("dark"');
    expect(source).toContain("GlobalSearchPalette");
    expect(source).toContain("AuthModal");
    expect(source).toContain("--home-header-height");
    expect(source).toContain("lg:hidden");
  });

  it("preserves existing product routes", () => {
    expect(source).toContain('href: "/"');
    expect(source).toContain("/dashboard");
    expect(source).toContain("/profile");
  });

  it("includes news in the shared public product nav", () => {
    const nav = readFileSync(join(process.cwd(), "src/lib/public/nav-v2.ts"), "utf8");
    expect(nav).toContain('href: "/news"');
    expect(nav).toContain('label: "اخبار"');
  });
});
