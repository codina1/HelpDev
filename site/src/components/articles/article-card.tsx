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

const BADGE_TONE: Record<string, string> = {
  ai: "bg-[#7C3AED]/20 text-[#C4B5FD] border-[#7C3AED]/35",
  frontend: "bg-[#0EA5E9]/15 text-[#7DD3FC] border-[#0EA5E9]/35",
  backend: "bg-[#059669]/15 text-[#6EE7B7] border-[#059669]/35",
  devops: "bg-[#0284C7]/15 text-[#7DD3FC] border-[#0284C7]/35",
  dotnet: "bg-[#6366F1]/15 text-[#A5B4FC] border-[#6366F1]/35",
  architecture: "bg-[#8B5CF6]/15 text-[#C4B5FD] border-[#8B5CF6]/35",
  programming: "bg-[#A855F7]/15 text-[#E9D5FF] border-[#A855F7]/35",
  tools: "bg-[#6366F1]/15 text-[#C7D2FE] border-[#6366F1]/35",
  security: "bg-[#EF4444]/15 text-[#FCA5A5] border-[#EF4444]/35",
};

type ArticleCardProps = {
  article: MarketplaceArticle;
};

/**
 * Article card — flex column · equal-height via grid · NO fixed card height
 * (fixed height was clipping row-2 text).
 */
export function ArticleCard({ article }: ArticleCardProps) {
  const badgeClass = BADGE_TONE[article.category] ?? BADGE_TONE.ai;

  return (
    <Link
      href={`/articles/${article.slug}`}
      className="group flex h-full min-h-0 min-w-0 flex-col rounded-[14px] border border-white/[0.08] bg-[#111827]/90 no-underline shadow-[0_4px_14px_rgba(2,6,23,0.28)] backdrop-blur-sm transition duration-300 hover:-translate-y-0.5 hover:border-[rgba(168,85,247,0.5)] hover:shadow-[0_0_24px_rgba(124,58,237,0.24)]"
      dir="rtl"
    >
      <div
        className={[
          "relative aspect-[16/9] w-full shrink-0 overflow-hidden rounded-t-[14px] bg-gradient-to-br",
          article.coverTone,
        ].join(" ")}
      >
        <span
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_70%_30%,rgba(168,85,247,0.28),transparent_55%)]"
          aria-hidden
        />
        <img
          src={article.coverImage}
          alt=""
          width={320}
          height={180}
          loading="lazy"
          decoding="async"
          className="absolute inset-0 h-full w-full object-cover mix-blend-screen transition duration-300 group-hover:scale-[1.04]"
        />
        <span
          className="pointer-events-none absolute inset-x-0 bottom-0 h-6 bg-gradient-to-t from-[#111827] to-transparent"
          aria-hidden
        />
        <span
          className={[
            "absolute right-2 top-2 z-[1] inline-flex items-center rounded-md border px-1.5 py-0.5 text-[9.5px] font-bold backdrop-blur-sm",
            badgeClass,
          ].join(" ")}
        >
          {article.categoryLabel}
        </span>
      </div>

      <div className="flex min-h-0 flex-1 flex-col gap-0.5 px-2.5 pb-2.5 pt-1.5">
        <h3 className="line-clamp-2 text-[12.5px] font-bold leading-[1.45] text-white transition group-hover:text-[#E9D5FF]">
          {article.title}
        </h3>
        <p className="line-clamp-2 text-[10.5px] leading-[1.55] text-[#94A3B8]">
          {article.description}
        </p>

        <div className="mt-auto flex items-center justify-between gap-1.5 border-t border-white/[0.06] pt-1.5 text-[10px] font-semibold text-[#94A3B8]">
          <span className="inline-flex items-center gap-2">
            <span className="inline-flex items-center gap-0.5">
              <EyeIcon className="h-3 w-3" />
              {formatViews(article.views)}
            </span>
            <span className="inline-flex items-center gap-0.5">
              <ClockIcon className="h-3 w-3" />
              {article.readingMinutes.toLocaleString("fa-IR")} دقیقه
            </span>
          </span>
          <span
            className="inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#7C3AED]/40 to-[#3B82F6]/20 text-[8px] font-bold text-white"
            title={article.author}
          >
            {article.authorInitials}
          </span>
        </div>
      </div>
    </Link>
  );
}
