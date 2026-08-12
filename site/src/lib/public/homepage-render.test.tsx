import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { KnowledgeShowcaseV2 } from "@/components/public/home/v2/knowledge-showcase-v2";
import { ToolExperienceV2 } from "@/components/public/home/v2/tool-experience-v2";
import { RoadmapExperienceV2 } from "@/components/public/home/v2/roadmap-experience-v2";
import { AiAssistantExperienceV2 } from "@/components/public/home/v2/ai-assistant-experience-v2";
import { KnowledgeGraphVisual } from "@/components/public/home/v2/knowledge-graph-visual";

describe("Sprint 50B — homepage render", () => {
  it("renders knowledge showcase with API-shaped items", () => {
    const html = renderToStaticMarkup(
      <KnowledgeShowcaseV2
        items={[
          {
            id: "1",
            title: "معماری Microservice",
            slug: "microservices",
            type: "Article",
            status: "Published",
            views: 10,
            saves: 1,
            createdAt: "2026-07-01T00:00:00Z",
          },
        ]}
      />,
    );
    expect(html).toContain("معماری Microservice");
    expect(html).toContain("/articles/microservices");
  });

  it("renders tool / roadmap / AI sections without fake catalog claims", () => {
    expect(renderToStaticMarkup(<ToolExperienceV2 tools={[]} />)).toContain("ابزاری برای نمایش نیست");
    expect(renderToStaticMarkup(<RoadmapExperienceV2 items={[]} />)).toContain("Frontend Engineer");
    expect(renderToStaticMarkup(<AiAssistantExperienceV2 />)).toContain("تحلیل توسط HelpDev AI");
    expect(renderToStaticMarkup(<KnowledgeGraphVisual />)).toContain("Articles");
  });
});
