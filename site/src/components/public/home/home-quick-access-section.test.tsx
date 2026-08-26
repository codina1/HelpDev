import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  QUICK_ACCESS_ITEMS,
  QuickAccessSection,
} from "@/components/public/home/QuickAccessSection";
import { HomeQuickAccessSection } from "@/components/public/home/home-quick-access-section";

describe("homepage quick access", () => {
  it("renders five premium navigation cards with 3D icons", () => {
    const html = renderToStaticMarkup(<QuickAccessSection />);
    expect(html).toContain("دسترسی سریع");
    expect(html).toContain("text-[28px]");
    expect(html).toContain("lg:grid-cols-5");
    expect(html).toContain("sm:grid-cols-3");
    expect(html).toContain("grid-cols-2");
    expect(html).toContain("gap-4");
    expect(html).toContain("min-h-[220px]");
    expect(html).toContain("w-full");
    expect(html).not.toContain("lg:w-[220px]");
    expect(html).not.toContain("h-[180px]");
    expect(html).toContain("rounded-[22px]");
    expect(html).toContain("hover:-translate-y-2");
    expect(html).toContain("hover:shadow-[0_0_35px_rgba(124,58,237,0.25)]");
    expect(html).toContain("group-hover:scale-110");
    expect(html).toContain("h-16 w-16");
    expect(html).toContain("linear-gradient(145deg, #111827, #080d1c)");
    expect(html).toContain("text-[#A855F7]");
    expect(html).toContain("absolute bottom-5 left-1/2");
    expect(html).toContain("h-6 w-6");
    expect(html).toContain("pb-[68px]");
    expect(html).toContain("group-hover:translate-y-1");
    expect(html).toContain('width="64"');
    expect(html).toContain('width="24"');
    expect(html).not.toContain("مسیرهای اصلی HelpDev");

    for (const item of QUICK_ACCESS_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
      expect(html).toContain(`href="${item.href}"`);
    }
  });

  it("keeps HomeQuickAccessSection as a thin alias", () => {
    const a = renderToStaticMarkup(<QuickAccessSection />);
    const b = renderToStaticMarkup(<HomeQuickAccessSection />);
    expect(a).toBe(b);
  });

  it("exposes the five requested destinations", () => {
    expect(QUICK_ACCESS_ITEMS.map((item) => item.href)).toEqual([
      "/news",
      "/toolbox",
      "/prompt-lab",
      "/roadmap",
      "/learning",
    ]);
  });
});
