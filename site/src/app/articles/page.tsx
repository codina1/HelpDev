import type { Metadata } from "next";
import { ArticlesCatalog } from "@/components/articles/articles-catalog";
import { ArticlesHero } from "@/components/articles/articles-hero";

export const metadata: Metadata = {
  title: "مقالات",
  description:
    "آخرین آموزش‌ها، بررسی ابزارها و تحلیل تکنولوژی‌های روز برنامه‌نویسی، هوش مصنوعی و توسعه نرم‌افزار",
};

export default function ArticlesPage() {
  return (
    <div className="bg-[#070b18]">
      <ArticlesHero />
      <ArticlesCatalog />
    </div>
  );
}
