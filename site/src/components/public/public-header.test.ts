import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("public homepage header", () => {
  const source = readFileSync(join(process.cwd(), "src/components/public/public-header.tsx"), "utf8");

  it("keeps sticky glass chrome with logo, nav, search, theme, and login", () => {
    expect(source).toContain('href="/"');
    expect(source).toContain("SITE.name");
    expect(source).toContain("PUBLIC_PRODUCTS_NAV");
    expect(source).toContain("ورود / ثبت‌نام");
    expect(source).toContain("ThemeToggle");
    expect(source).toContain("helpdev-public-theme");
    expect(source).toContain('classList.toggle("dark"');
    expect(source).toContain("GlobalSearchPalette");
    expect(source).toContain("AuthModal");
    expect(source).toContain("--home-header-height");
    expect(source).toContain("sm:hidden");
    expect(source).toContain("BrandMarkIcon");
    expect(source).toContain("backdrop-blur");
    expect(source).toContain("sticky");
  });

  it("uses the requested primary nav labels", () => {
    expect(source).toContain('label: "خانه"');
    const nav = readFileSync(join(process.cwd(), "src/lib/public/nav-v2.ts"), "utf8");
    expect(nav).toContain('label: "مقالات"');
    expect(nav).toContain('label: "یادگیری"');
    expect(nav).toContain('label: "Roadmap"');
    expect(nav).toContain('label: "Prompt Lab"');
    expect(nav).toContain('label: "ابزارها"');
    expect(nav).toContain('label: "اخبار"');
    expect(nav).toContain('href: "/prompt-lab"');
  });

  it("preserves auth routes for signed-in users", () => {
    expect(source).toContain("/dashboard");
    expect(source).toContain("/profile");
  });

  it("keeps mobile chrome to logo, search, and menu", () => {
    expect(source).toContain("pub-navbar-search");
    expect(source).toContain("باز کردن منو");
    expect(source).toContain("sm:hidden");
  });
});
