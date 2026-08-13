import { HomeHero } from "@/components/public/home/home-hero";
import { HomeStatsStrip } from "@/components/public/home/home-stats-strip";
import type { HomeStatItem } from "@/components/public/home/home-stat";
import { HomeWorkflowSection } from "@/components/public/home/home-workflow-section";
import { HomeValueSection } from "@/components/public/home/home-value-section";
import { HomePathsSection } from "@/components/public/home/home-paths-section";
import { HomeArticlesSection } from "@/components/public/home/home-articles-section";
import { HomeTrustSection } from "@/components/public/home/home-trust-section";
import { HomeNewsletterSection } from "@/components/public/home/home-newsletter-section";
import { InteractionRevealObserver } from "@/components/public/home/v2/interaction-reveal-observer";
import { listPublishedContent } from "@/lib/api/content";
import { listTools } from "@/lib/api/toolbox";
import { isArticleType, isRoadmapType, isToolContentType } from "@/lib/public/content-helpers";

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

/** Public homepage — core sections only, no duplicate v2 stack. */
export async function PublicHomePage() {
  const [content, tools] = await Promise.all([safeListContent(), safeListTools()]);

  const articles = content
    .filter((item) => isArticleType(item.type))
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const roadmaps = content.filter((item) => isRoadmapType(item.type));
  const contentTools = content.filter((item) => isToolContentType(item.type));
  const toolsCount = tools.length > 0 ? tools.length : contentTools.length;

  return (
    <div className="min-w-0 overflow-x-clip pb-10">
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
      <HomeValueSection />
      <HomePathsSection roadmaps={roadmaps} />
      <HomeArticlesSection articles={articles} />
      <HomeTrustSection />
      <HomeNewsletterSection />
    </div>
  );
}

function buildHomeStats(counts: {
  articles: number;
  paths: number;
  tools: number;
}): HomeStatItem[] {
  return [
    { id: "engineers", label: "اعضای فعال", value: 0, icon: "engineers" },
    { id: "articles", label: "مقالات فنی", value: counts.articles, icon: "articles" },
    { id: "paths", label: "مسیرهای یادگیری", value: counts.paths, icon: "paths" },
    { id: "tools", label: "ابزارهای کار", value: counts.tools, icon: "tools" },
    { id: "questions", label: "پرسش‌های پاسخ‌داده‌شده", value: 0, icon: "questions" },
  ];
}
