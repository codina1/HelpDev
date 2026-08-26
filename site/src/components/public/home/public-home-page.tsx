import { HomeHero } from "@/components/public/home/home-hero";
import { HomeQuickAccessSection } from "@/components/public/home/home-quick-access-section";
import { HomeHubSection } from "@/components/public/home/home-hub-section";
import { LatestArticlesSection } from "@/components/public/home/LatestArticlesSection";
import { HomeNewsSection } from "@/components/public/home/home-news-section";
import { HomePathsSection } from "@/components/public/home/home-paths-section";
import { HomeNewsletterSection } from "@/components/public/home/home-newsletter-section";
import { InteractionRevealObserver } from "@/components/public/home/v2/interaction-reveal-observer";
import { listPublishedContent } from "@/lib/api/content";
import { listTools } from "@/lib/api/toolbox";
import { isRoadmapType } from "@/lib/public/content-helpers";

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

function isPureArticle(type: string) {
  return type.toLowerCase() === "article";
}

function isNews(type: string) {
  return type.toLowerCase() === "news";
}

/**
 * Public homepage — Design Reference order only:
 * Hero → Quick Access → Hub (Toolbox + Prompt Lab) →
 * Articles → News → Paths → Newsletter (Footer is global).
 */
export async function PublicHomePage() {
  const [content, tools] = await Promise.all([safeListContent(), safeListTools()]);

  const articles = content
    .filter((item) => isPureArticle(item.type))
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const news = content
    .filter((item) => isNews(item.type))
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const roadmaps = content.filter((item) => isRoadmapType(item.type));

  return (
    <div className="min-w-0 overflow-x-clip bg-[#050816] pb-10">
      <InteractionRevealObserver />
      <HomeHero />
      <HomeQuickAccessSection />
      <HomeHubSection tools={tools} />
      <LatestArticlesSection articles={articles} />
      <HomeNewsSection items={news} />
      <HomePathsSection roadmaps={roadmaps} />
      <HomeNewsletterSection />
    </div>
  );
}
