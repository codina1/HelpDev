import Link from "next/link";
import type { MarketplaceArticle } from "@/data/articles";

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z"
        stroke="currentColor"
        strokeWidth="1.6"
      />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.6" />
      <path d="M12 8v4.5l3 1.8" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  );
}

function ArrowIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M15 6 9 12l6 6"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function formatViews(views: number): string {
  if (views >= 1000) {
    const value = views / 1000;
    const rounded = value >= 10 ? Math.round(value) : Math.round(value * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return views.toLocaleString("fa-IR");
}

type FeaturedArticleProps = {
  article: MarketplaceArticle;
};

/**
 * Featured article card — text LEFT (~55%) · cover RIGHT (~45%).
 * Whole card is one Link to the article detail page.
 */
export function FeaturedArticle({ article }: FeaturedArticleProps) {
  const title = article.title?.trim() || "بدون عنوان";
  const summary = article.description?.trim() || `نگاهی کوتاه به «${title}» در HelpDev.`;

  return (
    <Link
      href={`/articles/${article.slug}`}
      className="group relative block w-full cursor-pointer overflow-hidden rounded-[16px] border border-[rgba(139,92,246,0.28)] bg-[linear-gradient(135deg,rgba(17,24,39,0.98),rgba(11,18,36,0.94))] no-underline shadow-[0_0_40px_rgba(124,58,237,0.14)] transition duration-200 hover:-translate-y-1 hover:border-[rgba(167,139,250,0.55)] hover:shadow-[0_0_48px_rgba(124,58,237,0.28)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#8B5CF6]"
      aria-label={`مطالعه مقاله ${title}`}
    >
      <article
        className="grid w-full items-stretch md:h-[260px] md:grid-cols-[minmax(0,55%)_minmax(0,45%)]"
        dir="ltr"
      >
        {/* Text — visual LEFT */}
        <div
          className="relative z-[1] flex min-h-0 min-w-0 flex-col px-7 py-7 sm:px-8 sm:py-8 md:h-full"
          dir="rtl"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center gap-1.5 rounded-lg border border-[#A78BFA]/50 bg-gradient-to-l from-[#7C3AED]/45 to-[#6D28D9]/35 px-2.5 py-1 text-[11px] font-extrabold text-white shadow-[0_0_16px_rgba(124,58,237,0.35)]">
              <span aria-hidden className="text-[12px] leading-none text-[#FDE68A]">
                ★
              </span>
              مقاله ویژه
            </span>
            <span className="inline-flex items-center rounded-lg border border-[#3B82F6]/40 bg-[#3B82F6]/15 px-2.5 py-1 text-[11px] font-bold text-[#BFDBFE]">
              {article.categoryLabel}
            </span>
          </div>

          <h2 className="mt-3 w-full text-[19px] font-extrabold leading-[1.4] text-white sm:text-[21px] md:line-clamp-2 md:text-[22px]">
            {title}
          </h2>

          <p className="mt-2 w-full text-[13px] leading-7 text-[#CBD5E1] line-clamp-2 sm:text-[13.5px]">
            {summary}
          </p>

          <div className="mt-auto flex w-full flex-col gap-3.5 pt-4">
            <div className="flex w-full flex-wrap items-center gap-x-4 gap-y-2 text-[12px] font-semibold text-[#94A3B8]">
              <span className="inline-flex min-w-0 items-center gap-2">
                <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full border border-white/[0.12] bg-gradient-to-br from-[#7C3AED]/50 to-[#3B82F6]/30 text-[10px] font-bold text-white">
                  {article.authorInitials}
                </span>
                <span className="truncate">{article.author}</span>
              </span>
              <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
                <ClockIcon className="h-3.5 w-3.5 shrink-0 text-[#A78BFA]" />
                {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
              </span>
              <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
                <EyeIcon className="h-3.5 w-3.5 shrink-0 text-[#A78BFA]" />
                {formatViews(article.views)} بازدید
              </span>
            </div>

            <span className="inline-flex h-10 w-fit items-center gap-2 rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-4 text-[13px] font-bold text-white shadow-[0_0_18px_rgba(124,58,237,0.4)] transition duration-200 group-hover:brightness-110">
              مطالعه مقاله
              <ArrowIcon className="h-4 w-4 shrink-0" />
            </span>
          </div>
        </div>

        {/* Image — visual RIGHT */}
        <div
          className={[
            "relative order-first h-[200px] w-full overflow-hidden bg-gradient-to-br md:order-none md:h-full",
            article.coverTone,
          ].join(" ")}
        >
          <span
            className="pointer-events-none absolute -inset-6 bg-[radial-gradient(circle_at_60%_45%,rgba(139,92,246,0.35),transparent_62%)] blur-2xl"
            aria-hidden
          />
          <img
            src={article.coverImage}
            alt=""
            width={640}
            height={260}
            loading="eager"
            decoding="async"
            className="absolute inset-0 h-full w-full object-cover object-center transition duration-200 group-hover:scale-[1.03]"
          />
          <span
            className="pointer-events-none absolute inset-y-0 start-0 w-16 bg-gradient-to-r from-[#0B1224] via-[#0B1224]/55 to-transparent md:w-20"
            aria-hidden
          />
        </div>
      </article>
    </Link>
  );
}
