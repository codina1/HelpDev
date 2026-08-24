import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  HOME_QUICK_ACCESS_ITEMS,
  HomeQuickAccessSection,
} from "@/components/public/home/home-quick-access-section";

describe("homepage quick access", () => {
  it("renders five cards with titles, copy, and routes", () => {
    const html = renderToStaticMarkup(<HomeQuickAccessSection />);
    expect(html).toContain("دسترسی سریع");
    expect(html).toContain("مسیرهای اصلی HelpDev");
    expect(html).toContain("lg:grid-cols-5");
    expect(html).toContain("grid-cols-2");

    for (const item of HOME_QUICK_ACCESS_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
      expect(html).toContain(`href="${item.href}"`);
    }
  });

  it("exposes the five requested destinations", () => {
    expect(HOME_QUICK_ACCESS_ITEMS.map((item) => item.href)).toEqual([
      "/news",
      "/toolbox",
      "/prompt-lab",
      "/roadmap",
      "/learning",
    ]);
  });
});
