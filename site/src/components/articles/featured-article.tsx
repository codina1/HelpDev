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

/** Featured — text LEFT · image RIGHT (~42%) · ~200px · no clip. */
export function FeaturedArticle({ article }: FeaturedArticleProps) {
  return (
    <article className="group relative rounded-[16px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,rgba(17,24,39,0.96),rgba(15,23,42,0.9))] shadow-[0_0_36px_rgba(124,58,237,0.12)] backdrop-blur-xl">
      <div
        className="grid items-stretch overflow-hidden rounded-[16px] md:h-[188px] md:grid-cols-[minmax(0,1fr)_44%]"
        dir="ltr"
      >
        {/* Text — visual LEFT */}
        <div
          className="flex min-w-0 flex-col justify-center px-4 py-3.5 sm:px-5 md:py-3 md:ps-5 md:pe-3"
          dir="rtl"
        >
          <div className="flex flex-wrap items-center gap-1.5">
            <span className="inline-flex items-center rounded-lg border border-[#7C3AED]/40 bg-[#7C3AED]/20 px-2 py-0.5 text-[10px] font-bold text-[#E9D5FF]">
              ⭐ مقاله ویژه
            </span>
            <span className="inline-flex items-center rounded-lg border border-[#3B82F6]/35 bg-[#3B82F6]/15 px-2 py-0.5 text-[10px] font-bold text-[#BFDBFE]">
              {article.categoryLabel}
            </span>
          </div>

          <h2 className="mt-1.5 line-clamp-2 text-[17px] font-extrabold leading-[1.3] text-white sm:text-[19px] md:text-[20px]">
            {article.title}
          </h2>
          <p className="mt-1 line-clamp-2 text-[11.5px] leading-[1.55] text-[#94A3B8]">
            {article.description}
          </p>

          <div className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-[11px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-1.5">
              <span className="inline-flex h-6 w-6 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#7C3AED]/45 to-[#3B82F6]/25 text-[9px] font-bold text-white">
                {article.authorInitials}
              </span>
              {article.author}
            </span>
            <span className="inline-flex items-center gap-1">
              <ClockIcon className="h-3.5 w-3.5" />
              {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
            </span>
            <span className="inline-flex items-center gap-1">
              <EyeIcon className="h-3.5 w-3.5" />
              {formatViews(article.views)} بازدید
            </span>
          </div>

          <div className="mt-2">
            <Link
              href={`/articles/${article.slug}`}
              className="focus-ring inline-flex h-8 items-center justify-center rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-3.5 text-[12px] font-bold text-white no-underline shadow-[0_0_14px_rgba(124,58,237,0.35)] transition hover:brightness-110"
            >
              مطالعه مقاله
            </Link>
          </div>
        </div>

        {/* Image — visual RIGHT */}
        <div
          className={[
            "relative min-h-[140px] overflow-hidden bg-gradient-to-br md:min-h-0",
            article.coverTone,
          ].join(" ")}
        >
          <span
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_55%_40%,rgba(59,130,246,0.35),transparent_60%)]"
            aria-hidden
          />
          <img
            src={article.coverImage}
            alt=""
            width={440}
            height={200}
            loading="eager"
            decoding="async"
            className="absolute inset-0 h-full w-full object-cover mix-blend-screen transition duration-300 group-hover:scale-[1.03]"
          />
        </div>
      </div>
    </article>
  );
}
