import Link from "next/link";
import type { NewsArticle } from "@/types";

type PopularNewsSidebarProps = {
  articles: NewsArticle[];
};

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

/** Ranked popular list with thumbnails — fixed sidebar column. */
export function PopularNewsSidebar({ articles }: PopularNewsSidebarProps) {
  const popular = [...articles]
    .sort((a, b) => parseViews(b.views) - parseViews(a.views))
    .slice(0, 5);

  return (
    <section
      className="rounded-[20px] border border-white/[0.08] bg-[#111827]/90 p-5 shadow-[0_8px_28px_rgba(2,6,23,0.28)] backdrop-blur-xl"
      aria-labelledby="popular-news-heading"
    >
      <h2 id="popular-news-heading" className="text-[17px] font-extrabold text-white">
        محبوب‌ترین اخبار
      </h2>
      <ol className="mt-5 space-y-3.5">
        {popular.map((article, index) => (
          <li key={article.id}>
            <Link
              href={`#news-${article.id}`}
              className="group flex items-center gap-3 rounded-2xl border border-transparent p-1.5 text-start no-underline transition hover:border-white/[0.06] hover:bg-white/[0.03]"
            >
              <span className="relative h-16 w-16 shrink-0 overflow-hidden rounded-xl border border-white/[0.08] bg-[#080d1c] shadow-[0_0_16px_rgba(124,58,237,0.12)]">
                <img
                  src={article.image}
                  alt=""
                  width={64}
                  height={64}
                  decoding="async"
                  className="h-full w-full scale-110 object-cover opacity-95 transition duration-300 group-hover:scale-[1.15]"
                />
                <span className="absolute -start-1.5 -top-1.5 flex h-6 w-6 items-center justify-center rounded-lg bg-[#7C3AED] text-[11px] font-extrabold text-white shadow-[0_0_14px_rgba(124,58,237,0.55)]">
                  {index + 1}
                </span>
              </span>
              <span className="min-w-0 flex-1 py-0.5">
                <span className="line-clamp-2 text-[13px] font-bold leading-6 text-[#E2E8F0] transition group-hover:text-white">
                  {article.title}
                </span>
                <span className="mt-1.5 inline-flex items-center gap-1.5 text-[11px] font-medium text-[#64748B]">
                  <EyeIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
                  {article.views}
                </span>
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
