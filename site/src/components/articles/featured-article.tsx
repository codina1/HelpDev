import Link from "next/link";
import type { MarketplaceArticle } from "@/data/articles";

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" stroke="currentColor" strokeWidth="1.6" />
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
      <path d="M15 6 9 12l6 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
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
 * Featured article — full content column width.
 * Desktop: text ~55% (left) · image ~45% (right) · fixed ~230px height.
 * Mobile: image on top, text below.
 */
export function FeaturedArticle({ article }: FeaturedArticleProps) {
  return (
    <article className="group relative w-full overflow-hidden rounded-[16px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,rgba(17,24,39,0.96),rgba(15,23,42,0.9))] shadow-[0_0_36px_rgba(124,58,237,0.12)] backdrop-blur-xl">
      <div
        className="grid w-full items-stretch overflow-hidden rounded-[16px] md:h-[230px] md:grid-cols-[minmax(0,55%)_minmax(0,45%)]"
        dir="ltr"
      >
        {/* Text — visual LEFT (~55%) */}
        <div
          className="flex h-full min-h-0 min-w-0 flex-col p-6 sm:p-7 md:h-full"
          dir="rtl"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center gap-1 rounded-lg border border-[#7C3AED]/40 bg-[#7C3AED]/20 px-2.5 py-1 text-[11px] font-bold text-[#E9D5FF]">
              <span aria-hidden>⭐</span>
              مقاله ویژه
            </span>
            <span className="inline-flex items-center rounded-lg border border-[#3B82F6]/35 bg-[#3B82F6]/15 px-2.5 py-1 text-[11px] font-bold text-[#BFDBFE]">
              {article.categoryLabel}
            </span>
          </div>

          <h2 className="mt-3 w-full text-[18px] font-extrabold leading-[1.35] text-white sm:text-[20px] md:text-[22px] md:line-clamp-2">
            {article.title}
          </h2>

          <p className="mt-2 w-full text-[13px] leading-7 text-[#94A3B8] line-clamp-2">
            {article.description}
          </p>

          <div className="mt-3 flex w-full flex-wrap items-center gap-x-4 gap-y-2 text-[12px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-2">
              <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#7C3AED]/45 to-[#3B82F6]/25 text-[10px] font-bold text-white">
                {article.authorInitials}
              </span>
              <span className="whitespace-nowrap">{article.author}</span>
            </span>
            <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
              <ClockIcon className="h-3.5 w-3.5 shrink-0" />
              {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
            </span>
            <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
              <EyeIcon className="h-3.5 w-3.5 shrink-0" />
              {formatViews(article.views)} بازدید
            </span>
          </div>

          {/* dir=rtl → justify-end pins CTA to visual left (bottom-left of text pane) */}
          <div className="mt-auto flex w-full justify-end pt-4">
            <Link
              href={`/articles/${article.slug}`}
              className="focus-ring inline-flex h-10 items-stretch overflow-hidden rounded-xl border border-white/[0.14] bg-[#070B18]/90 text-[13px] font-bold text-white no-underline transition hover:border-[#8B5CF6]/45 hover:bg-[#0B1224]"
            >
              <span className="inline-flex items-center px-4">مطالعه مقاله</span>
              <span
                className="inline-flex w-10 items-center justify-center border-s border-white/[0.14] bg-white/[0.04]"
                aria-hidden
              >
                <ArrowIcon className="h-4 w-4" />
              </span>
            </Link>
          </div>
        </div>

        {/* Image — visual RIGHT (~45%) */}
        <div
          className={[
            "relative order-first min-h-[180px] w-full overflow-hidden bg-gradient-to-br md:order-none md:min-h-0 md:h-full",
            article.coverTone,
          ].join(" ")}
        >
          <img
            src={article.coverImage}
            alt=""
            width={640}
            height={230}
            loading="eager"
            decoding="async"
            className="absolute inset-0 h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
          />
        </div>
      </div>
    </article>
  );
}
