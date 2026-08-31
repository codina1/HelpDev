import type { NewsArticle } from "@/types";

type FeaturedNewsProps = {
  article: NewsArticle;
};

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 8v4.2l2.5 1.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
      <path d="M5 18.5h14" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function BookmarkIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M7 4.5h10a1 1 0 0 1 1 1V20l-6-3.2L6 20V5.5a1 1 0 0 1 1-1Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
    </svg>
  );
}

/** Featured: image left / content right, fixed 280px. */
export function FeaturedNews({ article }: FeaturedNewsProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group relative grid min-w-0 overflow-hidden rounded-[16px] border border-white/[0.08] bg-[#111827] shadow-[0_8px_30px_rgba(2,6,23,0.35)] md:h-[280px] md:grid-cols-2"
      dir="ltr"
    >
      <div className="relative aspect-[16/10] overflow-hidden bg-[#080d1c] md:aspect-auto md:h-full">
        <img
          src={article.image}
          alt=""
          width={560}
          height={280}
          loading="eager"
          fetchPriority="high"
          decoding="async"
          className="absolute inset-0 h-full w-full object-cover object-center transition duration-500 group-hover:scale-[1.03]"
        />
        <span className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827]/50 via-transparent to-transparent md:bg-gradient-to-r md:from-transparent md:to-[#111827]/30" aria-hidden />
      </div>

      <div className="relative flex flex-col justify-center gap-3 p-5 text-right sm:p-6 md:h-[280px] md:p-7" dir="rtl">
        <button
          type="button"
          className="absolute start-4 top-4 inline-flex h-9 w-9 items-center justify-center rounded-xl border border-white/[0.1] bg-white/[0.03] text-[#94A3B8] transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
          aria-label="افزودن به ذخیره‌ها"
        >
          <BookmarkIcon className="h-4 w-4" />
        </button>

        <span className="inline-flex w-fit items-center rounded-lg bg-[#7C3AED] px-2.5 py-1 text-[11px] font-bold text-white">
          {article.categoryLabel ?? article.tag}
        </span>

        <h2 className="line-clamp-2 pe-10 text-[18px] font-extrabold leading-7 text-white sm:text-[20px]">
          {article.title}
        </h2>
        <p className="line-clamp-2 text-[12.5px] leading-6 text-[#94A3B8]">
          {article.summary}
        </p>
        <div className="mt-1 flex items-center gap-5 text-[12px] font-semibold text-[#64748B]">
          <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
            <ClockIcon className="h-3.5 w-3.5 shrink-0 text-[#7C3AED]" />
            <bdi>{article.time}</bdi>
          </span>
          <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
            <BookIcon className="h-3.5 w-3.5 shrink-0 text-[#7C3AED]" />
            <bdi>{article.readTime}</bdi>
          </span>
        </div>
      </div>
    </article>
  );
}
