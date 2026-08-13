import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("public homepage composition", () => {
  const source = readFileSync(
    join(process.cwd(), "src/components/public/home/public-home-page.tsx"),
    "utf8",
  );

  it("keeps the core homepage and omits the extra stacked sections", () => {
    expect(source).toContain("HomeHero");
    expect(source).toContain("HomeStatsStrip");
    expect(source).toContain("HomeWorkflowSection");
    expect(source).toContain("HomeValueSection");
    expect(source).toContain("HomePathsSection");
    expect(source).toContain("HomeArticlesSection");
    expect(source).toContain("HomeTrustSection");
    expect(source).toContain("HomeNewsletterSection");
    expect(source).not.toContain("PersonalizedHero");
    expect(source).not.toContain("EngineeringIntelligenceSection");
    expect(source).not.toContain("AiWorkflowDemo");
    expect(source).not.toContain("KnowledgeShowcaseV2");
    expect(source).not.toContain("ToolExperienceV2");
    expect(source).not.toContain("RoadmapExperienceV2");
    expect(source).not.toContain("DeveloperJourneyTimeline");
    expect(source).not.toContain("EngineeringCaseStudies");
    expect(source).not.toContain("AiDecisionDemo");
    expect(source).not.toContain("DeveloperIdentitySection");
    expect(source).not.toContain("KnowledgeSearchSection");
  });
});
