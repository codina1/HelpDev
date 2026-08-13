import type { Metadata } from "next";
import { listPublishedContent } from "@/lib/api/content";
import { listTools } from "@/lib/api/toolbox";
import { isArticleType, isRoadmapType, isToolContentType } from "@/lib/public/content-helpers";
import { AboutHero, ABOUT_HERO_SUBTITLE } from "./about-hero";
import { AboutMission } from "./about-mission";
import { AboutStats } from "./about-stats";
import { AboutStory } from "./about-story";
import { AboutTeam } from "./about-team";

export const metadata: Metadata = {
  title: "درباره ما",
  description: ABOUT_HERO_SUBTITLE,
};

async function loadAboutCounts() {
  const [content, tools] = await Promise.all([
    listPublishedContent().catch(() => []),
    listTools().catch(() => []),
  ]);
  const articles = content.filter((item) => isArticleType(item.type)).length;
  const paths = content.filter((item) => isRoadmapType(item.type)).length;
  const contentTools = content.filter((item) => isToolContentType(item.type)).length;
  return {
    articles,
    paths,
    tools: tools.length > 0 ? tools.length : contentTools,
  };
}

export default async function AboutPage() {
  const counts = await loadAboutCounts();

  return (
    <>
      <AboutHero />
      <AboutMission />
      <AboutStory />
      <AboutStats counts={counts} />
      <AboutTeam />
    </>
  );
}
