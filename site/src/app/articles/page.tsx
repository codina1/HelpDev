import type { Metadata } from "next";
import { ArticlesCatalog } from "@/components/articles/articles-catalog";
import { ArticlesHero } from "@/components/articles/articles-hero";
import { PageErrorState } from "@/components/ui/page-error-state";
import { listPublishedContent } from "@/lib/api/content";
import { mapPublishedContentToMarketplace } from "@/lib/public/map-published-articles";

export const metadata: Metadata = {
  title: "مقالات",
  description:
    "آخرین آموزش‌ها، بررسی ابزارها و تحلیل تکنولوژی‌های روز برنامه‌نویسی، هوش مصنوعی و توسعه نرم‌افزار",
};

export const dynamic = "force-dynamic";

export default async function ArticlesPage() {
  try {
    const published = await listPublishedContent();
    const articles = mapPublishedContentToMarketplace(published);

    return (
      <div className="bg-[#070b18]">
        <ArticlesHero />
        <ArticlesCatalog articles={articles} />
      </div>
    );
  } catch (error) {
    return (
      <div className="bg-[#070b18] px-4 py-10">
        <PageErrorState error={error} />
      </div>
    );
  }
}
