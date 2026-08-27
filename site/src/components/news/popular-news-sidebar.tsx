import Link from "next/link";
import { formatNewsViewsShort } from "@/data/news-articles";
import type { NewsArticle } from "@/types";

type PopularNewsSidebarProps = {
  articles: NewsArticle[];
};

function FlameIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 3c1.8 2.2 3 4.2 3 6.2 0 1.2-.4 2.2-1 3 .8-.3 1.5-1 1.9-1.9.6 1.1.9 2.3.9 3.5A5.8 5.8 0 0 1 12 21a5.8 5.8 0 0 1-4.8-9.1C8.4 9.8 10 6.8 12 3Z"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
      <path
        d="M12 14.2c.9.4 1.5 1.3 1.5 2.4A2.4 2.4 0 0 1 12 19a2.4 2.4 0 0 1-1.8-4c.5.7 1.1 1 1.8-.8Z"
        fill="currentColor"
        opacity="0.85"
      />
    </svg>
  );
}

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

/**
 * Popular news sidebar matching reference:
 * [thumbnail] [title + summary + views] [rank badge]
 */
export function PopularNewsSidebar({ articles }: PopularNewsSidebarProps) {
  const popular = [...articles]
    .sort((a, b) => parseViews(b.views) - parseViews(a.views))
    .slice(0, 5);

  return (
    <section
      className="rounded-[18px] border border-white/[0.08] bg-[#0F172A]/95 p-4 shadow-[0_8px_28px_rgba(2,6,23,0.32)] backdrop-blur-xl sm:p-5"
      aria-labelledby="popular-news-heading"
      dir="rtl"
    >
      <h2
        id="popular-news-heading"
        className="flex items-center gap-2 text-[16px] font-extrabold text-white sm:text-[17px]"
      >
        <FlameIcon className="h-[18px] w-[18px] text-[#F97316]" />
        محبوب‌ترین اخبار
      </h2>

      <ol className="mt-4 space-y-2.5">
        {popular.map((article, index) => (
          <li key={article.id}>
            <Link
              href={`#news-${article.id}`}
              className="group flex items-center gap-2.5 rounded-2xl border border-white/[0.07] bg-[#111827]/70 p-2.5 no-underline transition duration-300 hover:border-[rgba(168,85,247,0.35)] hover:bg-[rgba(124,58,237,0.08)] sm:gap-3 sm:p-3"
            >
              {/* Rank on the start (right in RTL) — matches reference */}
              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg border border-[rgba(168,85,247,0.35)] bg-[#1E1035] text-[12px] font-extrabold text-[#C4B5FD] shadow-[0_0_12px_rgba(124,58,237,0.25)] sm:h-[30px] sm:w-[30px]">
                {index + 1}
              </span>

              <span className="min-w-0 flex-1 text-start">
                <span className="line-clamp-1 text-[13px] font-extrabold leading-5 text-white transition group-hover:text-[#E9D5FF]">
                  {article.title}
                </span>
                <span className="mt-0.5 line-clamp-1 text-[11px] leading-4 text-[#94A3B8]">
                  {article.summary}
                </span>
                <span className="mt-1 inline-flex items-center gap-1 text-[11px] font-semibold text-[#64748B]">
                  <EyeIcon className="h-3.5 w-3.5 text-[#A78BFA]" />
                  {formatNewsViewsShort(article.views)}
                </span>
              </span>

              <span className="h-11 w-11 shrink-0 overflow-hidden rounded-xl border border-white/[0.08] bg-[#080d1c] sm:h-[52px] sm:w-[52px]">
                <img
                  src={article.image}
                  alt=""
                  width={52}
                  height={52}
                  decoding="async"
                  className="h-full w-full scale-110 object-cover opacity-95 transition duration-300 group-hover:scale-[1.12]"
                />
              </span>
            </Link>
          </li>
        ))}
      </ol>
    </section>
  );
}

function parseViews(value: string): number {
  const western = value.replace(/[۰-۹]/g, (digit) =>
    String("۰۱۲۳۴۵۶۷۸۹".indexOf(digit)),
  );
  const normalized = western.replace(/[^\d.]/g, "");
  const num = Number.parseFloat(normalized);
  if (Number.isNaN(num)) return 0;
  if (/k/i.test(western) || /هزار/.test(value)) return num * 1000;
  return num;
}
