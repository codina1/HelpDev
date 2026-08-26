import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { formatDateFa, labelForContentType } from "@/lib/admin/content/content-mappers";
import type { ContentSummaryDto } from "@/lib/api/content";
import { publicHrefForContent } from "@/lib/public/content-helpers";
import { estimateReadingLabel, inferTechTags } from "@/lib/public/display-meta";
import { coverForHomeCategory } from "@/lib/public/home-covers";

export type LatestArticleItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  category: string;
  readingTime: string;
  date: string;
  image: string;
};

type LatestArticlesSectionProps = {
  articles?: ContentSummaryDto[];
};

export function categoryForLatestArticle(title: string, slug = "", type = "Article"): string {
  const hay = `${title} ${slug}`;
  if (/mcp/i.test(hay)) return "MCP";
  if (/claude\s*code|agent/i.test(hay)) return "AI Coding";
  if (/microservice|architect|modular|monolith|معمار|مرز ماژول/i.test(hay)) return "معماری";
  if (/\brag\b|llm|openai|embedding|هوش مصنوعی|\bai\b/i.test(hay)) return "هوش مصنوعی";
  if (/devops|docker|kubernetes|\bk8s\b|ci\/?cd|proxy|health/i.test(hay)) return "دواپس";
  if (/frontend|front-end|react|next\.?js|rtl|فرانت/i.test(hay)) return "فرانت‌اند";
  if (/backend|back-end|asp\.?\s*net|\.net|\bapi\b|outbox|بک/i.test(hay)) return ".NET";
  return labelForContentType(type);
}

export function descriptionForLatestArticle(title: string, slug = ""): string {
  const tags = inferTechTags(title, slug);
  if (tags.length > 0) {
    return `نگاهی فنی به ${tags.slice(0, 2).join(" و ")} در پایگاه دانش HelpDev.`;
  }
  return "مقاله فنی از دانش HelpDev برای تصمیم‌گیری سریع‌تر پیش از مطالعه کامل.";
}

export function mapLatestArticle(item: ContentSummaryDto): LatestArticleItem {
  const category = categoryForLatestArticle(item.title, item.slug, item.type);
  const cover = item.coverImage?.trim();
  return {
    id: item.id,
    title: item.title,
    description: descriptionForLatestArticle(item.title, item.slug),
    href: publicHrefForContent(item),
    category,
    readingTime: estimateReadingLabel(item.title),
    date: formatDateFa(item.createdAt),
    image: cover && cover.length > 0 ? cover : coverForHomeCategory(category),
  };
}

/** Real published articles only — never invent catalog rows. */
export function buildLatestArticles(articles: ContentSummaryDto[]): LatestArticleItem[] {
  return articles.slice(0, 5).map(mapLatestArticle);
}

/**
 * Latest Articles — Design Reference glass cards.
 * Desktop 5 · Tablet 3 · Mobile 1. API-backed only.
 */
export function LatestArticlesSection({ articles = [] }: LatestArticlesSectionProps) {
  const items = buildLatestArticles(articles);

  return (
    <section
      className="home-latest-articles relative py-10 sm:py-12 lg:py-14"
      aria-labelledby="latest-articles-heading"
    >
      <PublicContainer size="wide">
        <div className="mb-7 flex flex-wrap items-end justify-between gap-4 sm:mb-9">
          <div className="max-w-2xl text-start">
            <h2
              id="latest-articles-heading"
              className="text-[1.45rem] font-extrabold tracking-tight text-white sm:text-[1.7rem]"
            >
              جدیدترین مقالات
            </h2>
            <p className="mt-2.5 text-[14px] leading-7 text-[#94A3B8]">
              تازه‌ترین آموزش‌ها و تحلیل‌های دنیای توسعه، هوش مصنوعی و مهندسی نرم‌افزار
            </p>
          </div>
          <Link
            href="/articles"
            className="focus-ring inline-flex items-center gap-1.5 text-[13px] font-semibold text-[#A78BFA] no-underline transition hover:text-white"
          >
            مشاهده همه مقالات
            <ChevronIcon />
          </Link>
        </div>

        {items.length === 0 ? (
          <p className="rounded-[18px] border border-white/[0.08] bg-[#0B1224] px-5 py-10 text-center text-[14px] text-[#94A3B8]">
            هنوز مقاله‌ای منتشر نشده است.
          </p>
        ) : (
          <ul className="grid grid-cols-1 gap-4 sm:grid-cols-3 lg:grid-cols-5">
            {items.map((item) => (
              <LatestArticleCard key={item.id} item={item} />
            ))}
          </ul>
        )}
      </PublicContainer>
    </section>
  );
}

function LatestArticleCard({ item }: { item: LatestArticleItem }) {
  return (
    <li className="min-w-0">
      <Link
        href={item.href}
        className="group focus-ring flex h-full flex-col overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#0B1224] no-underline transition duration-300 hover:-translate-y-[6px] hover:border-[rgba(124,58,237,0.45)] hover:shadow-[0_0_32px_rgba(124,58,237,0.28)]"
      >
        <div className="relative aspect-[16/10] w-full shrink-0 overflow-hidden bg-[#080d1c]">
          <img
            src={item.image}
            alt=""
            width={640}
            height={400}
            decoding="async"
            className="absolute inset-0 h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
          />
          <span
            className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#0B1224]/75 via-transparent to-transparent"
            aria-hidden
          />
          <span className="absolute left-3 top-3 z-[1] rounded-full border border-[rgba(168,85,247,0.35)] bg-[rgba(11,18,36,0.72)] px-2.5 py-1 text-[11px] font-bold text-white backdrop-blur-md">
            {item.category}
          </span>
        </div>

        <div className="flex flex-1 flex-col px-4 pb-4 pt-3.5 text-start">
          <h3 className="line-clamp-2 text-[15px] font-bold leading-snug text-white sm:text-[16px]">
            {item.title}
          </h3>
          <p className="mt-2 line-clamp-2 text-[12px] leading-6 text-[#94A3B8] sm:text-[13px]">
            {item.description}
          </p>
          <div className="mt-auto flex flex-wrap items-center justify-between gap-2 pt-4 text-[11px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-1.5">
              <ClockIcon />
              {item.readingTime}
            </span>
            {item.date ? (
              <span className="inline-flex items-center gap-1.5">
                <CalendarIcon />
                {item.date}
              </span>
            ) : null}
          </div>
        </div>
      </Link>
    </li>
  );
}

function ChevronIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <path d="M15 6 9 12l6 6" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function ClockIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M12 7.5V12l3 2" />
    </svg>
  );
}

function CalendarIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <rect x="4" y="5" width="16" height="15" rx="2" />
      <path d="M8 3.5V7M16 3.5V7M4 10h16" />
    </svg>
  );
}
