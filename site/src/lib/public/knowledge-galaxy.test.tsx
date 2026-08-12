import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { KnowledgeGalaxy } from "@/components/experience/knowledge-galaxy";

describe("Sprint 50G — knowledge galaxy", () => {
  it("renders AI knowledge core with cross-layout hrefs and tooltips", () => {
    const html = renderToStaticMarkup(<KnowledgeGalaxy />);
    expect(html).toContain("هسته دانش AI");
    expect(html).toContain("HelpDev AI");
    expect(html).toContain('href="/articles"');
    expect(html).toContain('href="/toolbox"');
    expect(html).toContain('href="/roadmap"');
    expect(html).toContain('href="/learning"');
    expect(html).toContain("AI-curated engineering knowledge");
    expect(html).toContain("Developer productivity tools");
    expect(html).toContain("Personalized engineering paths");
    expect(html).toContain("Structured skill growth");
  });
});
