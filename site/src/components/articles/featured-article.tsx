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

/** Large horizontal featured article card — 55% content / 45% image. */
export function FeaturedArticle({ article }: FeaturedArticleProps) {
  return (
    <article
      className="group relative overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,rgba(17,24,39,0.96),rgba(15,23,42,0.9))] shadow-[0_0_50px_rgba(124,58,237,0.14)] backdrop-blur-xl"
      dir="rtl"
    >
      <div className="grid items-stretch md:grid-cols-[minmax(0,1.22fr)_minmax(220px,0.78fr)]">
        {/* Content — 55% */}
        <div className="flex flex-col justify-center p-6 sm:p-8 md:p-10">
          <div className="flex flex-wrap items-center gap-2.5">
            <span className="inline-flex items-center rounded-xl border border-[#7C3AED]/40 bg-[#7C3AED]/20 px-3 py-1.5 text-[11.5px] font-bold text-[#E9D5FF]">
              ⭐ مقاله ویژه
            </span>
            <span className="inline-flex items-center rounded-xl border border-[#3B82F6]/35 bg-[#3B82F6]/15 px-3 py-1.5 text-[11.5px] font-bold text-[#BFDBFE]">
              {article.categoryLabel}
            </span>
          </div>

          <h2 className="mt-4 text-[26px] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[30px] md:text-[32px]">
            {article.title}
          </h2>
          <p className="mt-3 max-w-[540px] text-[14px] leading-[1.85] text-[#94A3B8] sm:text-[15px]">
            {article.description}
          </p>

          <div className="mt-5 flex flex-wrap items-center gap-5 text-[13px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-2.5">
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#7C3AED]/45 to-[#3B82F6]/25 text-[11px] font-bold text-white">
                {article.authorInitials}
              </span>
              {article.author}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <ClockIcon className="h-4 w-4" />
              {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
            </span>
            <span className="inline-flex items-center gap-1.5">
              <EyeIcon className="h-4 w-4" />
              {formatViews(article.views)} بازدید
            </span>
          </div>

          <div className="mt-6">
            <Link
              href={`/articles/${article.slug}`}
              className="focus-ring inline-flex h-12 items-center justify-center rounded-2xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-7 text-[14px] font-bold text-white no-underline shadow-[0_0_22px_rgba(124,58,237,0.4)] transition hover:brightness-110"
            >
              مطالعه مقاله
            </Link>
          </div>
        </div>

        {/* Image — 45% — fully integrated */}
        <div
          className={[
            "relative min-h-[220px] overflow-hidden bg-gradient-to-br md:min-h-full",
            article.coverTone,
          ].join(" ")}
        >
          {/* Ambient glow */}
          <span
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_60%_40%,rgba(59,130,246,0.35),transparent_60%)]"
            aria-hidden
          />
          <span
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_40%_70%,rgba(124,58,237,0.25),transparent_55%)]"
            aria-hidden
          />
          {/* Image fills container, blends black bg away */}
          <img
            src={article.coverImage}
            alt=""
            width={440}
            height={320}
            loading="eager"
            decoding="async"
            className="absolute inset-0 h-full w-full object-cover mix-blend-screen drop-shadow-[0_20px_50px_rgba(15,23,42,0.6)] transition duration-300 group-hover:scale-[1.04]"
          />
          {/* Edge fade into content side */}
          <span
            className="pointer-events-none absolute inset-y-0 right-0 w-12 bg-gradient-to-l from-[rgba(17,24,39,0.7)] to-transparent md:block hidden"
            aria-hidden
          />
        </div>
      </div>
    </article>
  );
}
