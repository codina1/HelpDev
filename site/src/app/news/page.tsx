import type { Metadata } from "next";
import { NewsHero } from "@/components/news/news-hero";
import { NewsList } from "@/components/news/news-list";
import { NEWS_ARTICLES } from "@/data/news-articles";
import { listPublishedContent } from "@/lib/api/content";
import { mapPublishedNewsToArticles } from "@/lib/public/map-published-news";

export const metadata: Metadata = {
  title: "اخبار",
  description: "آخرین اخبار تکنولوژی، ابزارها و دنیای توسعه نرم‌افزار در HelpDev",
};

export const dynamic = "force-dynamic";

export default async function NewsPage() {
  let articles = NEWS_ARTICLES;
  try {
    const published = await listPublishedContent();
    const fromApi = mapPublishedNewsToArticles(published);
    if (fromApi.length > 0) articles = fromApi;
  } catch {
    // Keep catalog fallback when API is unavailable.
  }

  return (
    <>
      <NewsHero />
      <NewsList articles={articles} />
    </>
  );
}
