import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { AiDecisionDemo } from "@/components/public/home/v2/ai-decision-demo";
import { AiWorkflowDemo } from "@/components/public/home/v2/ai-workflow-demo";
import { DeveloperIdentitySection } from "@/components/public/home/v2/developer-identity-section";
import { DeveloperJourneyTimeline } from "@/components/public/home/v2/developer-journey-timeline";
import { EngineeringCaseStudies } from "@/components/public/home/v2/engineering-case-studies";
import { EngineeringIntelligenceSection } from "@/components/public/home/v2/engineering-intelligence-section";
import { PublicFooter } from "@/components/public/public-footer";
import {
  AI_WORKFLOW_STEPS,
  DEVELOPER_JOURNEY,
  ENGINEERING_STORIES,
  INTELLIGENCE_CARDS,
} from "@/lib/public/intelligence-showcase";

describe("Sprint 50G — Premium Interaction Layer", () => {
  it("renders Engineering Intelligence premium cards", () => {
    const html = renderToStaticMarkup(<EngineeringIntelligenceSection />);
    expect(html).toContain("Engineering Intelligence");
    for (const card of INTELLIGENCE_CARDS) {
      expect(html).toContain(card.title);
      expect(html).toContain(card.content);
    }
  });

  it("renders AI workflow V2 timeline labels", () => {
    const html = renderToStaticMarkup(<AiWorkflowDemo />);
    for (const step of AI_WORKFLOW_STEPS) {
      expect(html).toContain(step.code);
      expect(html).toContain(step.label);
    }
  });

  it("renders AI decision demo prompt and analyze CTA", () => {
    const html = renderToStaticMarkup(<AiDecisionDemo />);
    expect(html).toContain("با هوش مهندسی HelpDev، از سوال تا مسیر اجرا");
    expect(html).toContain("تحلیل توسط HelpDev AI");
    expect(html).toContain("ASP.NET Core");
  });

  it("renders engineering stories with Challenge Architecture Learning", () => {
    const html = renderToStaticMarkup(<EngineeringCaseStudies />);
    expect(html).toContain(ENGINEERING_STORIES[0].title);
    expect(html).toContain("Challenge");
    expect(html).toContain("Architecture");
    expect(html).toContain("Learning");
    expect(html).toContain("Netflix Scale Architecture");
  });

  it("renders developer identity selector", () => {
    const html = renderToStaticMarkup(<DeveloperIdentitySection />);
    expect(html).toContain("مسیر مهندسی خود را پیدا کنید");
    expect(html).toContain("Analyze My Path");
    expect(html).toContain("Beginner Developer");
    expect(html).toContain("Engineering Architect");
  });

  it("renders developer journey and premium footer structure", () => {
    const journey = renderToStaticMarkup(<DeveloperJourneyTimeline />);
    for (const stage of DEVELOPER_JOURNEY) {
      expect(journey).toContain(stage.label);
    }

    const footer = renderToStaticMarkup(<PublicFooter />);
    expect(footer).toContain("HelpDev");
    expect(footer).toContain("محصول");
    expect(footer).toContain("یادگیری");
    expect(footer).toContain("ابزارها");
    expect(footer).toContain("شرکت");
    expect(footer).toContain("خبرنامه مهندسی");
  });
});
