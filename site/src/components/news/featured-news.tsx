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
      <path
        d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <path d="M5 18.5h14" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function BookmarkIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M7 4.5h10a1 1 0 0 1 1 1V20l-6-3.2L6 20V5.5a1 1 0 0 1 1-1Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </svg>
  );
}

/** Large featured story card — image left / content right on desktop. */
export function FeaturedNews({ article }: FeaturedNewsProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group relative grid min-w-0 overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#111827] shadow-[0_0_30px_rgba(2,6,23,0.25)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_46px_rgba(2,6,23,0.55),0_0_36px_rgba(124,58,237,0.32)] sm:rounded-[20px] md:grid-cols-2 md:min-h-[280px] lg:min-h-[300px] xl:min-h-[330px]"
      dir="ltr"
    >
      <div className="relative aspect-[16/10] min-h-0 overflow-hidden bg-[#080d1c] sm:aspect-video md:aspect-auto md:h-full">
        <div
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_30%_35%,rgba(124,58,237,0.35),transparent_55%),radial-gradient(circle_at_80%_70%,rgba(56,189,248,0.12),transparent_50%)]"
          aria-hidden
        />
        <img
          src={article.image}
          alt=""
          width={640}
          height={360}
          decoding="async"
          className="absolute inset-0 h-full w-full scale-110 object-cover object-center opacity-90 transition duration-500 ease-out group-hover:scale-[1.16]"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827]/90 via-[#111827]/20 to-transparent md:bg-gradient-to-r md:from-transparent md:via-[#111827]/15 md:to-[#111827]/55"
          aria-hidden
        />
        <span className="absolute end-3 top-3 rounded-full border border-[rgba(168,85,247,0.42)] bg-[rgba(76,29,149,0.72)] px-2.5 py-1 text-[10px] font-bold text-[#F3E8FF] backdrop-blur-md sm:end-4 sm:top-4 sm:px-3 sm:text-[11px]">
          {article.tag}
        </span>
      </div>

      <div
        className="relative flex flex-col justify-center p-4 text-right sm:p-6 md:p-6 lg:p-7 xl:p-8"
        dir="rtl"
      >
        <button
          type="button"
          className="absolute start-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-xl border border-white/[0.1] bg-white/[0.04] text-[#94A3B8] transition hover:border-[rgba(168,85,247,0.4)] hover:text-white sm:start-4 sm:top-4"
          aria-label="افزودن به ذخیره‌ها"
        >
          <BookmarkIcon className="h-4 w-4" />
        </button>
        <p className="mb-2 text-[11px] font-bold tracking-wide text-[#A78BFA] sm:mb-3 sm:text-[12px]">
          خبر ویژه
        </p>
        <h2 className="pe-10 text-[18px] font-extrabold leading-8 text-white sm:text-[22px] sm:leading-9 lg:text-[24px] xl:text-[26px]">
          {article.title}
        </h2>
        <p className="mt-2 line-clamp-2 text-[13px] leading-6 text-[#94A3B8] sm:mt-3 sm:line-clamp-3 sm:text-[14px] sm:leading-7">
          {article.summary}
        </p>
        <div className="mt-4 flex flex-wrap items-center gap-x-4 gap-y-2 text-[11px] font-semibold text-[#64748B] sm:mt-5 sm:text-[12px]">
          <span className="inline-flex items-center gap-1.5">
            <ClockIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
            {article.time}
          </span>
          <span className="inline-flex items-center gap-1.5">
            <BookIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
            {article.readTime}
          </span>
        </div>
      </div>
    </article>
  );
}
