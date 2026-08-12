import type { Metadata } from "next";
import { ArticlesListing } from "@/components/public/articles/articles-listing";
import { listPublishedContent, type ContentSummaryDto } from "@/lib/api/content";
import { isArticleType } from "@/lib/public/content-helpers";

export const metadata: Metadata = {
  title: "مقالات",
  description: "مقالات آموزشی و اخبار منتشرشده HelpDev",
};

async function loadArticles(): Promise<ContentSummaryDto[]> {
  try {
    const all = await listPublishedContent();
    return all
      .filter((item) => isArticleType(item.type))
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  } catch {
    return [];
  }
}

export default async function ArticlesPage() {
  const items = await loadArticles();
  return <ArticlesListing items={items} />;
}
