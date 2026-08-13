import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HOME_VALUE_ITEMS, HomeValueSection } from "@/components/public/home/home-value-section";

describe("homepage value proposition", () => {
  it("renders four cards with icons, copy, and product links", () => {
    const html = renderToStaticMarkup(<HomeValueSection />);
    expect(html).toContain("چرا HelpDev؟");
    expect(html).toContain("text-start");
    expect(html).not.toContain("text-center");
    expect(html).toContain("home-value-grid");
    for (const item of HOME_VALUE_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
      expect(html).toContain(item.href);
    }
    expect(html).toContain("home-value-icon");
    expect(html).toContain("home-value-visual");
    expect(html).toContain("/home/cover-");
    expect(html).toContain("home-value-card-purple");
    expect(html).toContain("home-value-card-cyan");
    expect(html).toContain("home-value-card-ai");
    expect(html).toContain("home-value-card-blue");
  });
});
