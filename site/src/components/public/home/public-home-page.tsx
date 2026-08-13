import { HomeHero } from "@/components/public/home/home-hero";
import { HomeStatsStrip } from "@/components/public/home/home-stats-strip";
import type { HomeStatItem } from "@/components/public/home/home-stat";
import { HomeWorkflowSection } from "@/components/public/home/home-workflow-section";
import { PersonalizedHero } from "@/components/experience/personalized-hero";
import { AiDecisionDemo } from "@/components/public/home/v2/ai-decision-demo";
import { AiWorkflowDemo } from "@/components/public/home/v2/ai-workflow-demo";
import { DeveloperIdentitySection } from "@/components/public/home/v2/developer-identity-section";
import { DeveloperJourneyTimeline } from "@/components/public/home/v2/developer-journey-timeline";
import { EngineeringCaseStudies } from "@/components/public/home/v2/engineering-case-studies";
import { EngineeringIntelligenceSection } from "@/components/public/home/v2/engineering-intelligence-section";
import { InteractionRevealObserver } from "@/components/public/home/v2/interaction-reveal-observer";
import { KnowledgeSearchSection } from "@/components/public/home/v2/knowledge-search-section";
import { KnowledgeShowcaseV2 } from "@/components/public/home/v2/knowledge-showcase-v2";
import { RoadmapExperienceV2 } from "@/components/public/home/v2/roadmap-experience-v2";
import { ToolExperienceV2 } from "@/components/public/home/v2/tool-experience-v2";
import { TrustMetricsSection } from "@/components/public/home/v2/trust-metrics-section";
import { listPublishedContent } from "@/lib/api/content";
import { listTools } from "@/lib/api/toolbox";
import { isArticleType, isRoadmapType, isToolContentType } from "@/lib/public/content-helpers";
import { countAiGuideSignals } from "@/lib/public/display-meta";

async function safeListContent() {
  try {
    return await listPublishedContent();
  } catch {
    return [];
  }
}

async function safeListTools() {
  try {
    return await listTools();
  } catch {
    return [];
  }
}

/** Sprint 50G — Premium Interaction Layer homepage. */
export async function PublicHomePage() {
  const [content, tools] = await Promise.all([safeListContent(), safeListTools()]);

  const articles = content
    .filter((item) => isArticleType(item.type))
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const latest =
    articles.length > 0
      ? articles
      : [...content].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const roadmaps = content.filter((item) => isRoadmapType(item.type));
  const contentTools = content.filter((item) => isToolContentType(item.type));
  const toolsCount = tools.length > 0 ? tools.length : contentTools.length;
  const aiGuides = countAiGuideSignals(content);

  return (
    <div className="pb-10">
      <InteractionRevealObserver />
      <HomeHero />
      <HomeStatsStrip
        items={buildHomeStats({
          articles: articles.length,
          paths: roadmaps.length,
          tools: toolsCount,
        })}
      />
      <HomeWorkflowSection />
      <PersonalizedHero />
      <TrustMetricsSection
        metrics={[
          {
            label: "Engineering Articles",
            value: articles.length > 0 ? articles.length : latest.length,
            hint: "مقالات منتشرشده",
          },
          {
            label: "Learning Paths",
            value: roadmaps.length,
            hint: "نقشه‌های راه",
          },
          {
            label: "Developer Tools",
            value: toolsCount,
            hint: "ابزارهای در دسترس",
          },
          {
            label: "AI Guides",
            value: aiGuides,
            hint: "محتوای مرتبط با AI",
          },
        ]}
      />
      <EngineeringIntelligenceSection />
      <AiWorkflowDemo />
      <KnowledgeShowcaseV2 items={latest.slice(0, 7)} />
      <ToolExperienceV2 tools={tools} contentTools={contentTools} />
      <RoadmapExperienceV2 items={roadmaps} />
      <DeveloperJourneyTimeline />
      <EngineeringCaseStudies publishedExamples={articles.slice(0, 2)} />
      <AiDecisionDemo />
      <DeveloperIdentitySection />
      <KnowledgeSearchSection />
    </div>
  );
}

function buildHomeStats(counts: {
  articles: number;
  paths: number;
  tools: number;
}): HomeStatItem[] {
  return [
    { id: "engineers", label: "مهندسان فعال", value: 0, icon: "engineers" },
    { id: "articles", label: "مقالات فنی", value: counts.articles, icon: "articles" },
    { id: "paths", label: "مسیرهای یادگیری", value: counts.paths, icon: "paths" },
    { id: "tools", label: "ابزارهای مهندسی", value: counts.tools, icon: "tools" },
    { id: "questions", label: "پرسش‌های پاسخ‌داده‌شده", value: 0, icon: "questions" },
  ];
}
