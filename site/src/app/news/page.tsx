import type { Metadata } from "next";
import { NewsHero } from "@/components/news/news-hero";
import { NewsList } from "@/components/news/news-list";
import { NEWS_ARTICLES } from "@/data/news-articles";

export const metadata: Metadata = {
  title: "اخبار",
};

export default function NewsPage() {
  return (
    <>
      <NewsHero />
      <NewsList articles={NEWS_ARTICLES} />
    </>
  );
}
