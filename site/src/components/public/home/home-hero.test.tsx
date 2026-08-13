import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeHero } from "@/components/public/home/home-hero";

describe("homepage hero", () => {
  it("renders Persian copy, CTAs, and knowledge nodes without stats", () => {
    const html = renderToStaticMarkup(<HomeHero />);
    expect(html).toContain("از پرسش تا ساخت");
    expect(html).toContain("با هوش HelpDev");
    expect(html).toContain("/home/hero-scene.svg");
    expect(html).toContain("شروع یادگیری");
    expect(html).toContain("از AI بپرس");
    expect(html).toContain("/learning");
    expect(html).toContain("/learning/assistant");
    expect(html).toContain("Articles");
    expect(html).toContain("Tools");
    expect(html).toContain("Learning");
    expect(html).toContain("Roadmaps");
    expect(html).toContain("/articles");
    expect(html).toContain("/toolbox");
    expect(html).toContain("/roadmap");
    expect(html).toContain("HelpDev AI");
    expect(html).toContain("دانش منتشرشده");
    expect(html).toContain("مسیر یادگیری");
    expect(html).toContain("دستیار AI");
    expect(html).not.toContain("Engineering Articles");
    expect(html).not.toContain("Trust");
  });
});
