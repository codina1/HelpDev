import type { Metadata } from "next";
import { ArticlesListing } from "@/components/public/articles/articles-listing";
import { listPublishedContent, type ContentSummaryDto } from "@/lib/api/content";
import { isArticleType } from "@/lib/public/content-helpers";

export const metadata: Metadata = {
  title: "مقالات",
  description: "مقالات آموزشی و اخبار منتشرشده HelpDev",
};

// Important: this page should reflect newly published content quickly.
// Without this, Next may statically cache the empty published list at build time.
export const dynamic = "force-dynamic";

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
