import { HomeHero } from "@/components/public/home/home-hero";
import { HomeQuickAccessSection } from "@/components/public/home/home-quick-access-section";
import { HomeWorkflowSection } from "@/components/public/home/home-workflow-section";
import { HomeValueSection } from "@/components/public/home/home-value-section";
import { HomePathsSection } from "@/components/public/home/home-paths-section";
import { HomeArticlesSection } from "@/components/public/home/home-articles-section";
import { HomeTrustSection } from "@/components/public/home/home-trust-section";
import { HomeNewsletterSection } from "@/components/public/home/home-newsletter-section";
import { InteractionRevealObserver } from "@/components/public/home/v2/interaction-reveal-observer";
import { listPublishedContent } from "@/lib/api/content";
import { isArticleType, isRoadmapType } from "@/lib/public/content-helpers";

async function safeListContent() {
  try {
    return await listPublishedContent();
  } catch {
    return [];
  }
}

/** Public homepage — core sections only, no duplicate v2 stack. */
export async function PublicHomePage() {
  const content = await safeListContent();

  const articles = content
    .filter((item) => isArticleType(item.type))
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  const roadmaps = content.filter((item) => isRoadmapType(item.type));

  return (
    <div className="min-w-0 overflow-x-clip pb-10">
      <InteractionRevealObserver />
      <HomeHero />
      <HomeQuickAccessSection />
      <HomeWorkflowSection />
      <HomeValueSection />
      <HomePathsSection roadmaps={roadmaps} />
      <HomeArticlesSection articles={articles} />
      <HomeTrustSection />
      <HomeNewsletterSection />
    </div>
  );
}
