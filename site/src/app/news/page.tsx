import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { NewsList } from "@/components/news/news-list";
import { NEWS_ARTICLES } from "@/data/news-articles";

export const metadata: Metadata = {
  title: "اخبار",
};

export default function NewsPage() {
  return (
    <>
      <PageHeader
        title="اخبار"
        description="تازه‌ترین خبرهای دنیای برنامه‌نویسی در React، دات‌نت، هوش مصنوعی و DevOps."
      />
      <NewsList articles={NEWS_ARTICLES} />
    </>
  );
}
