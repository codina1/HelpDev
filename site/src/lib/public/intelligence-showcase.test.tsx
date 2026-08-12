import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { AiWorkflowDemo } from "@/components/public/home/v2/ai-workflow-demo";
import { DeveloperJourneyTimeline } from "@/components/public/home/v2/developer-journey-timeline";
import { EngineeringCaseStudies } from "@/components/public/home/v2/engineering-case-studies";
import { EngineeringIntelligenceSection } from "@/components/public/home/v2/engineering-intelligence-section";
import {
  AI_WORKFLOW_STEPS,
  DEVELOPER_JOURNEY,
  ENGINEERING_STORIES,
  INTELLIGENCE_CARDS,
} from "@/lib/public/intelligence-showcase";

describe("Sprint 50F/50G — Intelligence Showcase compatibility", () => {
  it("renders Engineering Intelligence cards", () => {
    const html = renderToStaticMarkup(<EngineeringIntelligenceSection />);
    expect(html).toContain("Engineering Intelligence");
    expect(html).toContain(INTELLIGENCE_CARDS[0].title);
  });

  it("renders AI workflow pipeline stages", () => {
    const html = renderToStaticMarkup(<AiWorkflowDemo />);
    for (const step of AI_WORKFLOW_STEPS) {
      expect(html).toContain(step.label);
      expect(html).toContain(step.titleFa);
    }
  });

  it("renders developer journey stages", () => {
    const html = renderToStaticMarkup(<DeveloperJourneyTimeline />);
    for (const stage of DEVELOPER_JOURNEY) {
      expect(html).toContain(stage.label);
      expect(html).toContain(stage.titleFa);
    }
  });

  it("renders engineering stories", () => {
    const html = renderToStaticMarkup(
      <EngineeringCaseStudies
        publishedExamples={[
          {
            id: "1",
            title: "معماری Microservice",
            slug: "microservices",
            type: "Article",
            status: "Published",
            views: 1,
            saves: 0,
            createdAt: "2026-07-01T00:00:00Z",
          },
        ]}
      />,
    );
    expect(html).toContain(ENGINEERING_STORIES[0].title);
    expect(html).toContain("Challenge");
    expect(html).toContain("معماری Microservice");
    expect(html).toContain("Published");
  });
});
