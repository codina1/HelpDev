import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { AboutHero, ABOUT_HERO_SUBTITLE, ABOUT_HERO_TITLE } from "./about-hero";

describe("about page hero", () => {
  it("renders the required title, subtitle, and AI visual without extra sections", () => {
    const html = renderToStaticMarkup(<AboutHero />);
    expect(html).toContain(ABOUT_HERO_TITLE.split(" ")[0]);
    expect(html).toContain("ما در HelpDev");
    expect(html).toContain("آینده مهندسی نرم‌افزار را می‌سازیم");
    expect(html).toContain(ABOUT_HERO_SUBTITLE);
    expect(html).toContain("HelpDev AI");
    expect(html).not.toContain("چرا HelpDev");
    expect(html).not.toContain("آخرین مقالات");
  });
});
