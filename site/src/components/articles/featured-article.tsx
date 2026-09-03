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

/** Large horizontal featured article card. */
export function FeaturedArticle({ article }: FeaturedArticleProps) {
  return (
    <article
      className="group relative overflow-hidden rounded-[18px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,rgba(17,24,39,0.95),rgba(15,23,42,0.88))] shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-xl"
      dir="rtl"
    >
      <div className="grid items-stretch md:grid-cols-[minmax(0,1.15fr)_minmax(220px,0.85fr)]">
        <div className="flex flex-col justify-center p-5 sm:p-7 md:p-8">
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-lg border border-[#7C3AED]/40 bg-[#7C3AED]/20 px-2.5 py-1 text-[11px] font-bold text-[#E9D5FF]">
              مقاله ویژه
            </span>
            <span className="inline-flex items-center rounded-lg border border-[#3B82F6]/35 bg-[#3B82F6]/15 px-2.5 py-1 text-[11px] font-bold text-[#BFDBFE]">
              {article.categoryLabel}
            </span>
          </div>

          <h2 className="mt-3 text-[22px] font-extrabold leading-9 tracking-tight text-white sm:text-[26px]">
            {article.title}
          </h2>
          <p className="mt-2 max-w-[520px] text-[13.5px] leading-7 text-[#94A3B8] sm:text-[14px]">
            {article.description}
          </p>

          <div className="mt-4 flex flex-wrap items-center gap-4 text-[12.5px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-2">
              <span className="inline-flex h-7 w-7 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#7C3AED]/45 to-[#3B82F6]/25 text-[10px] font-bold text-white">
                {article.authorInitials}
              </span>
              {article.author}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <ClockIcon className="h-3.5 w-3.5" />
              {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
            </span>
            <span className="inline-flex items-center gap-1.5">
              <EyeIcon className="h-3.5 w-3.5" />
              {formatViews(article.views)} بازدید
            </span>
          </div>

          <div className="mt-5">
            <Link
              href={`/articles/${article.slug}`}
              className="focus-ring inline-flex h-11 items-center justify-center rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[13.5px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110"
            >
              مطالعه مقاله
            </Link>
          </div>
        </div>

        <div
          className={[
            "relative flex min-h-[200px] items-center justify-center overflow-hidden bg-gradient-to-br md:min-h-full",
            article.coverTone,
          ].join(" ")}
        >
          <span
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_60%_40%,rgba(59,130,246,0.35),transparent_60%)]"
            aria-hidden
          />
          <img
            src={article.coverImage}
            alt=""
            width={180}
            height={180}
            loading="eager"
            decoding="async"
            className="relative h-[140px] w-[140px] object-contain drop-shadow-[0_16px_40px_rgba(15,23,42,0.55)] transition duration-300 group-hover:scale-105 sm:h-[160px] sm:w-[160px]"
          />
        </div>
      </div>
    </article>
  );
}
