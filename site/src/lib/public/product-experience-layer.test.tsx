import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  AiFeatureCard,
  ArticleCardPro,
  RoadmapCardPro,
  ToolCardPro,
} from "@/components/public/pro";
import { TrustMetricsSection } from "@/components/public/home/v2/trust-metrics-section";
import {
  countAiGuideSignals,
  inferTechTags,
  softAiSummary,
  softUseCases,
  structuralRoadmapStatuses,
} from "@/lib/public/display-meta";

describe("Sprint 50E — Product Experience Layer", () => {
  it("renders Intelligence ArticleCard with AI summary", () => {
    const html = renderToStaticMarkup(
      <ArticleCardPro
        title="ASP.NET Microservices"
        href="/articles/aspnet"
        category="مقاله"
        readingTime="۸ دقیقه"
        difficulty="متوسط"
        tags={[".NET"]}
        aiSummary={softAiSummary("ASP.NET Microservices", "aspnet")}
        featured
      />,
    );
    expect(html).toContain("ASP.NET Microservices");
    expect(html).toContain("AI summary");
    expect(html).toContain("بینش AI");
    expect(html).toContain(".NET");
  });

  it("renders roadmap locked/unlocked and completion chrome", () => {
    const html = renderToStaticMarkup(
      <RoadmapCardPro
        title="Frontend"
        href="/roadmap"
        level="میانی"
        nodes={[{ label: "React" }, { label: "Next.js" }, { label: "Testing" }, { label: "Deploy" }]}
      />,
    );
    expect(html).toContain("قفل");
    expect(html).toContain("جاری");
    expect(html).toContain("تکمیل");
    expect(html).toContain("میانی");
  });

  it("renders trust metrics from provided counts", () => {
    const html = renderToStaticMarkup(
      <TrustMetricsSection
        metrics={[
          { label: "Engineering Articles", value: 12 },
          { label: "Learning Paths", value: 3 },
          { label: "Developer Tools", value: 8 },
          { label: "AI Guides", value: 2 },
        ]}
      />,
    );
    expect(html).toContain("Engineering Articles");
    expect(html).toContain("Learning Paths");
    expect(html).toContain("Developer Tools");
    expect(html).toContain("AI Guides");
  });

  it("keeps tool rating placeholder and structural helpers honest", () => {
    expect(
      renderToStaticMarkup(
        <ToolCardPro title="Cursor" href="/toolbox" category="AI" useCases={["بهره‌وری AI"]} />,
      ),
    ).toContain("امتیاز به‌زودی");

    expect(renderToStaticMarkup(<AiFeatureCard title="دستیار" description="توضیح" />)).toContain(
      "Engineering Intelligence",
    );

    expect(structuralRoadmapStatuses(4)).toEqual([
      "completed",
      "current",
      "unlocked",
      "locked",
    ]);
    expect(inferTechTags("Hello world", "hello")).toEqual([]);
    expect(softUseCases("ai-tools")).toContain("بهره‌وری AI");
    expect(
      countAiGuideSignals([
        { title: "Prompt engineering", slug: "prompt-ai" },
        { title: "CSS grid", slug: "css" },
      ]),
    ).toBe(1);
  });
});
