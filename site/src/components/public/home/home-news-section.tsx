import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { formatDateFa, labelForContentType } from "@/lib/admin/content/content-mappers";
import type { ContentSummaryDto } from "@/lib/api/content";
import { publicHrefForContent } from "@/lib/public/content-helpers";
import { categoryForHomeArticle } from "@/components/public/home/home-articles-section";

const FALLBACK_NEWS = [
  {
    id: "n1",
    title: "انتشار به‌روزرسانی‌های MCP برای ابزارهای توسعه‌دهنده",
    href: "/news",
    category: "ابزارها",
    date: "",
  },
  {
    id: "n2",
    title: "مسیرهای جدید یادگیری AI Engineering در HelpDev",
    href: "/news",
    category: "یادگیری",
    date: "",
  },
  {
    id: "n3",
    title: "بهبود Prompt Lab با قالب‌های Cursor و Claude",
    href: "/news",
    category: "Prompt Lab",
    date: "",
  },
  {
    id: "n4",
    title: "گزارش تازه‌های اکوسیستم .NET و ASP.NET Core",
    href: "/news",
    category: ".NET",
    date: "",
  },
] as const;

type HomeNewsSectionProps = {
  items?: ContentSummaryDto[];
};

/** Compact latest-news cards — title, date, category. */
export function HomeNewsSection({ items = [] }: HomeNewsSectionProps) {
  const cards =
    items.length > 0
      ? items.slice(0, 4).map((item) => ({
          id: item.id,
          title: item.title,
          href: publicHrefForContent(item),
          category: categoryForHomeArticle(item.title, item.slug, item.type),
          date: formatDateFa(item.createdAt),
        }))
      : FALLBACK_NEWS.map((item) => ({ ...item }));

  return (
    <PublicSection
      className="home-news home-reveal"
      bare
      aria-labelledby="home-news-heading"
    >
      <PublicContainer size="wide">
        <div className="mb-5 flex flex-wrap items-end justify-between gap-3 sm:mb-6">
          <div>
            <p className="text-[12px] font-bold tracking-wide text-[#06B6D4]">اخبار</p>
            <h2
              id="home-news-heading"
              className="mt-1 text-[1.25rem] font-extrabold text-white sm:text-[1.4rem]"
            >
              آخرین اخبار
            </h2>
          </div>
          <Link
            href="/news"
            className="focus-ring text-[12px] font-semibold text-[#94A3B8] no-underline hover:text-white"
          >
            همه اخبار
          </Link>
        </div>

        <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {cards.map((item) => (
            <li key={item.id}>
              <Link
                href={item.href}
                className="focus-ring flex h-full flex-col rounded-2xl border border-white/[0.08] bg-[#0B1224] p-4 no-underline transition hover:-translate-y-1 hover:border-[rgba(124,58,237,0.4)] hover:shadow-[0_0_24px_rgba(124,58,237,0.18)]"
              >
                <span className="inline-flex w-fit rounded-full border border-[rgba(124,58,237,0.35)] bg-[rgba(124,58,237,0.12)] px-2.5 py-0.5 text-[10px] font-bold text-[#C4B5FD]">
                  {item.category || labelForContentType("News")}
                </span>
                <h3 className="mt-3 line-clamp-3 text-[13px] font-bold leading-6 text-white">
                  {item.title}
                </h3>
                <p className="mt-auto pt-3 text-[11px] font-semibold text-[#64748B]">
                  {item.date || "تازه"}
                </p>
              </Link>
            </li>
          ))}
        </ul>
      </PublicContainer>
    </PublicSection>
  );
}
